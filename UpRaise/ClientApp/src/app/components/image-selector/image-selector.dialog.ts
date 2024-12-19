import { Component, Inject, ChangeDetectorRef, OnDestroy, OnInit, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { fuseAnimations } from '@fuse/animations';
//import { ProfileService } from 'app/main/pages/profile/profile.service';
import { ViewChild } from '@angular/core';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ImageCropperModule } from 'ngx-image-cropper';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
//import { NgxUiLoaderService } from 'ngx-ui-loader';
import { IUser } from 'app/models/user';
import { UserService } from 'app/core/user/user.service';
import { FuseUtilsService } from '@fuse/services/utils/utils.service';
import { UploadService } from '../../modules/admin/funding/new-campaign/upload.service';
import { HttpEventType, HttpResponse } from '@angular/common/http';

export interface DialogData {
    fileName: string;
    roundCropper: boolean;
    imageChangedEvent: any;
    saveType: number;
}

interface UserProfilePictureFileInfo extends File {
}



@Component({
    selector: 'image-selector',
    templateUrl: './image-selector.dialog.html',
    styleUrls: ['./image-selector.dialog.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class ImageSelectorDialog implements OnInit, OnDestroy, AfterViewInit {
    public imageChangedEvent: any = '';
    public croppedImage: any = '';
    public showCropper = false;
    public roundCropper = false;
    private subUser: any;
    private currentUser: IUser;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    @ViewChild(ImageCropperComponent) imageCropper: ImageCropperComponent;

    constructor(
        //private ngxService: NgxUiLoaderService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _uploadService: UploadService,
        public matDialogRef: MatDialogRef<ImageSelectorDialog>,
        private _userService: UserService,
        private _fuseUtilsService: FuseUtilsService,
        @Inject(MAT_DIALOG_DATA) public data: DialogData
    ) {
        this.imageChangedEvent = this.data.imageChangedEvent;
        //this.ngxService.start();

        this.roundCropper = this.data.roundCropper
    }

    public ngOnInit(): void {
        // Subscribe to user changes
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.currentUser = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });
    }


    public ngAfterViewInit() {
        this.imageCropper.roundCropper = this.data.roundCropper;
    }

    public ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    public imageCropped(event: ImageCroppedEvent) {
        this.croppedImage = event.base64;
    }

    public errorEventHandler(event: ErrorEvent) {
        console.log('errorEventHandler');
    }

    /*
    public successEventHandler(e: SuccessEvent) {
        //console.log('successEventHandler');
        this.matDialogRef.close({ Result: 'save', Filename: e.files[0].name })
    }
    */

    public imageLoaded() {
        // show cropper
        this.showCropper = true;

        //console.log('Image loaded')
        //this.ngxService.stop();
    }
    public cropperReady() {
        // cropper ready
    }
    public loadImageFailed() {
        // show message
    }

    /*
    public uploadProgressEventHandler(e: UploadProgressEvent) {
        console.log('uploadProgressEventHandler');
        //console.log(e.files[0].name + ' is ' + e.percentComplete.valueOf() + ' uploaded');

        //this._progressBarService.setMessage("Uploading " + e.files[0].name);
        //this._progressBarService.setValue(e.percentComplete.valueOf());
    }

    public uploadEventHandler(e: UploadEvent) {

        const saveFileJSON = JSON.stringify({
            SaveType: this.data.saveType,
            Filename: e.files[0].name
        });

        e.data = {
            SaveFile: saveFileJSON
        };
    }
    */
    public uploadCompleted(e: any) {
    }


    private b64toFile(dataURI): File {
        // convert the data URL to a byte string
        const byteString = atob(dataURI.split(',')[1]);

        // pull out the mime type from the data URL
        const mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0]

        // Convert to byte array
        const ab = new ArrayBuffer(byteString.length);
        const ia = new Uint8Array(ab);
        for (let i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        // Create a blob that looks like a file.
        const blob = new Blob([ab], { 'type': mimeString });
        blob['lastModifiedDate'] = (new Date()).toISOString();


        //blob['name'] = this.data.fileName.split('.').slice(0, -1).join('.');
        blob['name'] = this.data.fileName;

        // Figure out what extension the file should have
        switch (blob.type) {
            case 'image/jpeg':
                blob['extension'] = '.jpg';
                break;
            case 'image/png':
                blob['extension'] = '.png';
                break;
        }
        // cast to a File
        return <File>blob;
    }


    public save(): void {
        var file = this.b64toFile(this.croppedImage);

        const saveFileJSON = JSON.stringify({
            SaveType: 2, /* User Profile Picture */
            Filename: file.name
        });

        var formData = new FormData();
        formData.append('SaveFile', saveFileJSON);



        //this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Uploading;

        this._uploadService.upload(file, formData).subscribe(
            (event: any) => {
                if (event.type === HttpEventType.UploadProgress) {
                    //this.progressInfos[idx].value = Math.round(100 * event.loaded / event.total);
                    //console.log(this.progressInfos[idx].value);
                } else if (event instanceof HttpResponse) {
                    //this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Success;
                    //const msg = 'Uploaded the file successfully: ' + file.name;
                    //this.progressInfos[idx].message = msg;
                    //this.saveIfAllUploadsFinished();
                    this._userService.refreshUser();
                    this.matDialogRef.close();
                }
            },
            (err: any) => {
                //this.progressInfos[idx].value = 0;
                //this.progressInfos[idx].status = SaveComponentUploadStatusesEnum.Error;
                //const msg = 'Could not upload the file: ' + file.name;
                //this.progressInfos[idx].message = msg;

                //this.saveIfAllUploadsFinished();
            });

    }

}
