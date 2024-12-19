import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { IPublicCampaignDetail } from '../home.types';
import { HomeService } from '../home.service';
import { EnumDTO } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { ActivatedRoute } from '@angular/router';
import { ContributionService } from './contribution.service';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../../core/user/user.service';
import { contributionStepsData } from './contribution.types';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { ContributionTypes } from './contribution.enums';

@Component({
    selector: 'contribution',
    templateUrl: './contribution.component.html',
    styleUrls: ['./contribution.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [{
        provide: STEPPER_GLOBAL_OPTIONS, useValue: { displayDefaultIndicatorType: false }
    }]
})
export class ContributionComponent implements OnInit, OnDestroy {

    ContributionTypes = ContributionTypes;
    contributionType: ContributionTypes = ContributionTypes.Crypto;

    firstFormGroup: FormGroup;
    secondFormGroup: FormGroup;

    @ViewChild('contributionSteps', { static: true }) contributionSteps: MatTabGroup;

    isSignedIn: boolean;
    campaign: IPublicCampaignDetail;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    contributionStepsData =  contributionStepsData;
    /**
     * Constructor
     */
    constructor(
        @Inject(DOCUMENT) private _document: Document,
        private _activatedRoute: ActivatedRoute,
        private _homeService: HomeService,
        private _contributionService: ContributionService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private dialog: MatDialog,
        private _formBuilder: FormBuilder
    ) {


    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {

        this.firstFormGroup = this._formBuilder.group({
            firstCtrl: ['', Validators.required]
        });
        this.secondFormGroup = this._formBuilder.group({
            secondCtrl: ['', Validators.required]
        });


        this.isSignedIn = this._activatedRoute.snapshot.data['isSignedIn'];

        this._homeService.campaign$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((publicCampaignDetail: IPublicCampaignDetail) => {

                // Get the course
                this.campaign = publicCampaignDetail;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }


}
