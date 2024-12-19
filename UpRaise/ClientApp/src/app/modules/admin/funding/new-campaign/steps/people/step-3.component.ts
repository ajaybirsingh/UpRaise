import { AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { WizardEventTypes } from '../../new-campaign.types';

export interface DistributionTerm {
    label: string;
    removable: boolean;
}

@Component({
    selector: 'people-step-3',
    templateUrl: './step-3.component.html',
})
export class Step3Component {
    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();
    @Input() public stepData: FormGroup;
    
    @ViewChild('addTerm', { static: true })
    public addTerm: ElementRef;

    constructor(private _formBuilder: FormBuilder) {
    }

    private createItem(label: string): FormGroup {
        return this._formBuilder.group({
            label: label,
            removable: true,
        });
    }

    get distributionTerms() {
        var formArray = this.stepData.get('distributionTerms') as FormArray;
        return formArray;
    }

    private processDistributionTerms() {
        const value = (this.addTerm.nativeElement.value || '').trim();
        if (value) {

            this.distributionTerms.push(this.createItem(value));
        }

        this.addTerm.nativeElement.value = '';
    }

    public onKeyDown(pressedKey) {
        if (pressedKey.key === "Enter") {
            this.processDistributionTerms();
        }
    }

    public removeOption(distributionTerm: DistributionTerm) {

        const index = this.distributionTerms.value.indexOf(distributionTerm);

        if (index >= 0) {
            this.distributionTerms.value.splice(index, 1);
        }
    }


    public prev() {
        this.processDistributionTerms();
        this.WizardEvent.emit(WizardEventTypes.Previous);
    }


    public next() {
        this.processDistributionTerms();
        this.WizardEvent.emit(WizardEventTypes.Next);
    }
    
}
