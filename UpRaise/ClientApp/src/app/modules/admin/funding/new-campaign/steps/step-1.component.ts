import { AfterViewInit, Component, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CampaignTypes } from '../../../../../shared/enums';
import { WizardEventTypes } from '../new-campaign.types';



@Component({
    selector: 'step-1',
    templateUrl: './step-1.component.html',
})
export class Step1Component implements AfterViewInit {

    CampaignTypes = CampaignTypes;
    //campaignType: undefined;

    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();
    @Input() public stepData: FormGroup;

    campaignTypeId: number;

    constructor() {
    }

    public ngAfterViewInit(): void {
    }

    public toggleVisibility(): void {
    }

    public updateCampaign(_campaignType: CampaignTypes) {

        //this.campaignType = _campaignType;
        //this.campaignTypeChange.emit(this.campaignType);
    }

    public continue() {
        this.WizardEvent.emit(WizardEventTypes.Next);
    }
}
