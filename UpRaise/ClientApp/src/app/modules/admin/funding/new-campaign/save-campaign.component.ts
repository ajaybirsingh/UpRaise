import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import swal, { SweetAlertOptions } from 'sweetalert2';
import { StatusEnum } from 'app/models';
import { Component, OnInit, ViewEncapsulation, ViewChild, Inject } from '@angular/core';
import { FuseUtilsService } from '@fuse/services/utils/utils.service';
import { FormBuilder, FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { CampaignService } from '../../../../core/campaigns/campaigns.service';
import { newArray } from '@angular/compiler/src/util';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CampaignTypes } from '../../../../shared/enums';
import { UploadService } from './upload.service';
import { HttpEventType, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IEmailBeneficiaryRequestDTO } from '../../dashboards/campaign/campaign.types';


@Component({
    selector: 'save-campaign',
    templateUrl: './save-campaign.component.html',
    encapsulation: ViewEncapsulation.None,
    styleUrls: ['./save-campaign.styles.css']
})
export class SaveCampaignComponent implements OnInit {

    public SaveComponentUploadStatusesEnum = SaveComponentUploadStatusesEnum;
    public SaveComponentStatusesEnum = SaveComponentStatusesEnum;
    public _saveComponentStatus: SaveComponentStatusesEnum = null;
    public progressInfos: ICampaignFileProgressInfo[];
    public savingStatus?: SaveComponentUploadStatusesEnum = null;
    public emailBeneficiaryStatus?: SaveComponentUploadStatusesEnum = null;
    private _campaignId?: number;


    /**
     * Constructor
     */
    constructor(
        private _formBuilder: FormBuilder,
        private _fuseUtilsService: FuseUtilsService,
        private _campaignService: CampaignService,
        private _uploadService: UploadService,
        private _dialogRef: MatDialogRef<SaveCampaignComponent>,
        @Inject(MAT_DIALOG_DATA) private data: { campaignType: CampaignTypes, data: any, headerPhotoFile: any, photoFiles: any, videoFiles: any, transactionId: string }) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        this.uploadFiles();
    }


    public ok() {
        this._dialogRef.close({ status: true, id: this._campaignId });
    }

    public cancel() {
        this._dialogRef.close(false);
    }

    private uploadFiles() {

        const files: ICampaignFileInfo[] = [];

        if (this.data.headerPhotoFile) {
            this.data.headerPhotoFile.fileInfo.typeId = 3; /* header file */
            files.push(this.data.headerPhotoFile.fileInfo);
        }

        if (this.data.photoFiles) {
            this.data.photoFiles.forEach((fileData: any) => {
                fileData.fileInfo.typeId = 1; /* photo file */
                files.push(fileData.fileInfo);
            });
        }

        if (this.data.videoFiles) {
            this.data.videoFiles.forEach((fileData: any) => {
                fileData.fileInfo.typeId = 2; /* video file */
                files.push(fileData.fileInfo);
            });
        }

        if (files.length > 0) {

            this.progressInfos = [];

            //
            //initialize array
            //
            for (let i = 0; i < files.length; i++) {
                const file = files[i];

                const fileProgressInfo: ICampaignFileProgressInfo = { value: 0, status: SaveComponentUploadStatusesEnum.Pending, fileName: file.name };
                this.progressInfos[i] = fileProgressInfo;
            }

            //now kick off uploads after array is initialize
            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                this.upload(i, file);
            }
        }
        else
            this.saveCampaignData();
    }


    upload(idx: number, file: ICampaignFileInfo): void {

        if (file) {

            const saveFileJSON = JSON.stringify({
                SaveType: 1, /* Campaign File */
                UID: file.uid,
                TypeId: file.typeId,
                TransactionId: this.data.transactionId,
                Filename: file.name
            });

            var formData = new FormData();
            formData.append('SaveFile', saveFileJSON);

            this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Uploading;

            this._uploadService.upload(file, formData).subscribe(
                (event: any) => {
                    if (event.type === HttpEventType.UploadProgress) {
                        this.progressInfos[idx].value = Math.round(100 * event.loaded / event.total);
                        //console.log(this.progressInfos[idx].value);
                    } else if (event instanceof HttpResponse) {
                        this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Success;
                        const msg = 'Uploaded the file successfully: ' + file.name;
                        this.progressInfos[idx].message = msg;

                        this.saveIfAllUploadsFinished();
                    }
                },
                (err: any) => {
                    this.progressInfos[idx].value = 0;
                    this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Error;
                    const msg = 'Could not upload the file: ' + file.name;
                    this.progressInfos[idx].message = msg;

                    this.saveIfAllUploadsFinished();
                });
        }
    }

    private saveIfAllUploadsFinished() {

        //
        //abort from method if there is any file still being uploaded or pending
        //
        for (let i = 0; i < this.progressInfos.length; i++) {
            const progressInfo = this.progressInfos[i];
            if (progressInfo.status != SaveComponentUploadStatusesEnum.Success &&
                progressInfo.status != SaveComponentUploadStatusesEnum.Error)
                return; //we still have a file pending or uploading
        }

        //
        //at this point all the files are uploaded so we need to check the status
        //
        for (let i = 0; i < this.progressInfos.length; i++) {
            const progressInfo = this.progressInfos[i];

            if (progressInfo.status == SaveComponentUploadStatusesEnum.Error) {
                this._saveComponentStatus = SaveComponentStatusesEnum.Error;
                return; //we still have a file pending or uploading
            }
        }

        //
        //if we made it here the files have been uploaded so now we can save the data
        //
        this.saveCampaignData();

    }


    private saveCampaignData() {

        this.savingStatus = SaveComponentUploadStatusesEnum.Uploading;
        this._campaignService.saveCampaign(this.data.data).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success) {

                    this.savingStatus = SaveComponentUploadStatusesEnum.Success;
                    this._campaignId = resultDTO.data.id;
                    //swal.fire('Save Organization Campaign', "Saved Organization Campaign.", 'success');

                    this.emailBeneficiary();
                }
                else {
                    this.savingStatus = SaveComponentUploadStatusesEnum.Error;
                    this._saveComponentStatus = SaveComponentStatusesEnum.Error;
                    /*
                    var errorMessage = '';
 
                    if (resultDTO.data && resultDTO.data.length) {
                        errorMessage = "There was an error saving your campaign.\nPlease fix the following and retry: \n";
                        for (var i = 0; i < resultDTO.data.length; i++) {
                            errorMessage += '- ' + resultDTO.data[i] + '\n';
                        }
                    }
                    else
                        errorMessage = resultDTO.message;
                    swal.fire('Save Organization Campaign', errorMessage, 'error');
                    */
                }
            },
            error => {
                console.error(error)
                this.savingStatus = SaveComponentUploadStatusesEnum.Error;
                this._saveComponentStatus = SaveComponentStatusesEnum.Error;
            },
            () => {

            });
    }

    private emailBeneficiary() {

        this.emailBeneficiaryStatus = SaveComponentUploadStatusesEnum.Uploading;

        this._campaignService.emailBeneficiary(<IEmailBeneficiaryRequestDTO>{ campaignId: this._campaignId }).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success) {
                    this.emailBeneficiaryStatus = SaveComponentUploadStatusesEnum.Success;
                    this._saveComponentStatus = SaveComponentStatusesEnum.Success;
                }
                else {
                    this.emailBeneficiaryStatus = SaveComponentUploadStatusesEnum.Error;
                    this._saveComponentStatus = SaveComponentStatusesEnum.Error;
                }
            },
            error => {
                //console.error(error)
                this.emailBeneficiaryStatus = SaveComponentUploadStatusesEnum.Error;
                this._saveComponentStatus = SaveComponentStatusesEnum.Error;
            },
            () => {

            });
    }

}



enum SaveComponentStatusesEnum {
    Success = 1,
    Error = 2
}

enum SaveComponentUploadStatusesEnum {
    Pending = 1,
    Uploading = 2,
    Success = 3,
    Error = 4
}

interface ICampaignFileInfo extends File {
    typeId: number;
    uid: string;
    //transactionId: string;
}

interface ICampaignFileProgressInfo {
    value: number;
    fileName: string;
    status: SaveComponentUploadStatusesEnum;
    message?: string;
}
