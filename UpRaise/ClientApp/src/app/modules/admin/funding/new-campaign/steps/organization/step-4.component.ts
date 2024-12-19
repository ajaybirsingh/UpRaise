import { AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { WizardEventTypes } from '../../new-campaign.types';

export interface CampaignCondition {
    label: string;
    removable: boolean;
}

@Component({
    selector: 'org-step-4',
    templateUrl: './step-4.component.html',
})
export class Step4Component {
    @Input() public stepData: FormGroup;
    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();

    @ViewChild('addTerm', { static: true })
    public addTerm: ElementRef;
    /*
    public formatOptions: any = {
        style: "currency",
        currency: "USD",
        format: "c",
        decimals: 2,
        currencyDisplay: "name",
    };
    */
    constructor(private _formBuilder: FormBuilder) {
    }



    private createItem(label: string): FormGroup {
        return this._formBuilder.group({
            label: label,
            removable: true,
        });
    }

    get campaignConditions() {
        var formArray = this.stepData.get('campaignConditions') as FormArray;
        return formArray;
    }

    private processCampaignConditions() {
        const value = (this.addTerm.nativeElement.value || '').trim();
        if (value) {

            this.campaignConditions.push(this.createItem(value));
        }

        this.addTerm.nativeElement.value = '';
    }

    public onKeyDown(pressedKey) {
        if (pressedKey.key === "Enter") {
            this.processCampaignConditions();
        }
    }

    public removeOption(campaignCondition: CampaignCondition) {

        //const index = this.campaignConditions.value.indexOf(campaignCondition);
        const index = this.campaignConditions.value.findIndex(term => term.label === campaignCondition.label);

        if (index >= 0) {
            //this.images.removeAt(this.images.value.findIndex(image => image.id === 502))
            this.campaignConditions.removeAt(index);
        }

    }


    public prev() {
        this.processCampaignConditions();
        this.WizardEvent.emit(WizardEventTypes.Previous);
    }


    public next() {
        this.processCampaignConditions();
        this.WizardEvent.emit(WizardEventTypes.Next);
    }


}
