import { AfterViewInit, Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { WizardEventTypes } from '../../new-campaign.types';

@Component({
    selector: 'people-step-6',
    templateUrl: './step-6.component.html',
})
export class Step6Component implements AfterViewInit {
    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();

    @Input() public stepData: FormGroup;

    public ngAfterViewInit(): void {
    }

    public prev() {
        this.WizardEvent.emit(WizardEventTypes.Previous);
    }


    public next() {
        this.WizardEvent.emit(WizardEventTypes.GoLive);
    }
}
