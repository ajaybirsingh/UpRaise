import { AfterViewInit, Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormArray, FormGroup } from '@angular/forms';
import muxjs from 'mux.js'
import { FileUploadStates, ICampaignPreview, WizardEventTypes } from '../../new-campaign.types';
import { v4 as uuidv4 } from 'uuid';
import { CampaignTypes } from '../../../../../../shared/enums';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { LandingHomeDetailsComponent } from '../../../../../landing/home/details/details.component';

@Component({
    selector: 'org-step-6',
    templateUrl: './step-6.component.html',
    styleUrls: ['./step-6.component.css']
})
export class Step6Component {
    @Input() public stepData: FormGroup;
    @Input() public formData: FormGroup;
    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();

    constructor(
        private _formBuilder: FormBuilder,
        private matDialog: MatDialog,
    ) {
    }


    public prev() {
        this.WizardEvent.emit(WizardEventTypes.Previous);
    }


    public next() {
        this.WizardEvent.emit(WizardEventTypes.Next);
    }


    selectFiles(event: any): void {
        var filesArray = Array.from(event.target.files);
        filesArray.forEach((file: any) => {

            file.uid = uuidv4();

            const reader = new FileReader();

            reader.onload = (e: any) => {

                this.photos.push(this.createItem({
                    fileInfo: file,
                    state: FileUploadStates.Added,
                    src: e.target.result  //Base64 string for preview image
                }));
            };

            reader.readAsDataURL(file);
        });
    }

    get photos(): FormArray {
        return this.stepData.get('photos') as FormArray;
    };

    private createItem(data): FormGroup {
        return this._formBuilder.group(data);
    }

    removeImage(idx: number, photo: any) {
        this.photos.removeAt(idx);
    }


    removeHeaderImage(idx: number, photo: any) {
        var data = this.stepData.controls['headerPhoto'].setValue('');
    }

    addheaderPicture(event: any): void {

        var file = event.target.files[0];
        file.uid = uuidv4();

        const reader = new FileReader();

        reader.onload = (e: any) => {
            //console.log(e.target.result);
            this.stepData.controls['headerPhoto'].setValue({ fileInfo: file, state: FileUploadStates.Added, src: <string>e.target.result });;
        };

        reader.readAsDataURL(file);
    }

    preview() {
        var data = this.createPreviewData();

        this.matDialog.open(LandingHomeDetailsComponent, {
            width: '90vw',
            maxHeight: '90vh',
            data: data,
            autoFocus: false,
            closeOnNavigation: true,
            disableClose: true,
            hasBackdrop: true
        });
    }

    createPreviewData(): ICampaignPreview {
        var campaignPreview: ICampaignPreview = {
            type: CampaignTypes.Organization,

            category: '',

            name: this.formData.controls["step2"].get("name").value,
            description: this.formData.controls["step2"].get("description").value,

            headerPhoto: this.stepData.controls['headerPhoto'].value,

            beneficiaryOrganization: '',
            location: '',
            contactName: '',
        };

        return campaignPreview;
    }

}
