import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, Optional, ViewChild, ViewEncapsulation } from '@angular/core';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { IPublicCampaignDetail } from '../home.types';
import { HomeService } from '../home.service';
import { EnumDTO, IUser, StatusEnum } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { ActivatedRoute } from '@angular/router';
import { DetailService } from './details.service';
import { CampaignAnalyticTypes } from './details.enums';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../../../core/user/user.service';
import 'quill-emoji/dist/quill-emoji';
import Quill from 'quill'
import { Router } from '@angular/router';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CampaignTypes } from '../../../../shared/enums';


@Component({
    selector: 'campaign-details',
    templateUrl: './details.component.html',
    styleUrls: ['./details.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingHomeDetailsComponent implements OnInit, OnDestroy, AfterViewInit {
    isSignedIn: boolean;

    campaign: IPublicCampaignDetail;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    public IsPreview: boolean = false;

    /**
     * Constructor
     */
    constructor(
        @Optional() private dialogRef: MatDialogRef<LandingHomeDetailsComponent>,
        @Optional() @Inject(MAT_DIALOG_DATA) private formCampaignData,
        @Inject(DOCUMENT) private _document: Document,
        private _activatedRoute: ActivatedRoute,
        private _homeService: HomeService,
        private _detailService: DetailService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private dialog: MatDialog,
        private _router: Router
    ) {

        //this.isSignedIn = this._activatedRoute.snapshot.data['isSignedIn'];
        this.IsPreview = formCampaignData != null;
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    quillInit() {

        var view = new Quill('#quill-viewer', {
            modules: {
                'emoji-toolbar': false,
                //theme:"snow"
            },
            scrollingContainer: '#scrolling-container'
        });

        view.disable();

        //viewer.root.innerHTML = this.stepData.get('description').value;
    }

    /**
     * On init
     */
    ngOnInit(): void {

        if (this.IsPreview) {
            this.isSignedIn = false;
            this.campaign = {
                id: -1,
                transactionId: '',
                type: this.formCampaignData.type,
                category: 'category',

                numberOfFollowers: 0,
                numberOfVisits: 0,
                numberOfShares: 0,
                numberOfDonors: 0,

                name: this.formCampaignData.name,
                description: this.formCampaignData.description,
                headerPictureURL: '',
                beneficiaryOrganizationName: 0,

                location: 'Calgary, AB',

                createdByUserFullName: '',
                createdByUserAliasId: '',

                showProfilePic: false,

                isUserFollowing: false,

                numberOfContributions: 0,

                seoFriendlyURL: null,

                createdAt: new Date(),
                updatedAt: new Date(),

                fundraisingGoals: 0,

                featured: false,

                completed: false,

                daysLeft: undefined,

                fundedPreviously: false,

                fundedPercentage: 0,

                fundingAmountToDate: 0,

                fundingLastDateTime: undefined,

                photos: [],
                videos: [],

                isCurrentSignedInUserCreator: true
            };

            if (!this.formCampaignData.headerPhoto) {
                this.campaign.headerPictureURL = `/assets/images/campaigns/default_${(this.campaign.type == CampaignTypes.Organization ? "organization" : "people")}.jpg`;
            }

            this._userService.user$.subscribe((user: IUser) => {
                this.campaign.showProfilePic = user.pictureURL ? true : false;

                this.campaign.createdByUserFullName = `${user.firstName} ${user.lastName}`;
                this.campaign.createdByUserAliasId = user.aliasId;
                //this.campaign.updatedAt = user.updatedAt;
            });


        }
        else {
            this.isSignedIn = this._activatedRoute.snapshot.data['isSignedIn'];


            // Get the course
            this._homeService.campaign$
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe((publicCampaignDetail: IPublicCampaignDetail) => {

                    // Get the course
                    this.campaign = publicCampaignDetail;

                    if (this.campaign)
                        this._detailService.addAnalytics(this.campaign.id, this.campaign.type, CampaignAnalyticTypes.Visit);

                    // Mark for check
                    this._changeDetectorRef.markForCheck();
                });
        }

    }
    public ngAfterViewInit(): void {
        this.quillInit();
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------
    isEmptyOrSpaces(str) {
        return str === null || str.match(/^ *$/) !== null;
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    shareClick() {
        const dialogRef = this.dialog.open(ShareDialog, {
            //panelClass: 'mail-ngrx-compose-dialog'
            //disableClose: true,
            data: { publicCampaignDetail: this.campaign }
        });

        dialogRef.afterClosed().subscribe(result => {

            if (result && result.wasShared) {
                this._detailService.addAnalytics(this.campaign.id, this.campaign.type, CampaignAnalyticTypes.Share);
            }
        });
    }

    contactClick() {
        const dialogRef = this.dialog.open(ContactDialog, {
            //panelClass: 'mail-ngrx-compose-dialog'
            data: { campaignId: this.campaign.id, campaignType: this.campaign.type, userAliasId: this.campaign.createdByUserAliasId, showProfilePic: this.campaign.showProfilePic, fullName: this.campaign.createdByUserFullName }
        });

        dialogRef.afterClosed().subscribe(result => {

            if (result) {
                //if (result.Result.toLowerCase() === "save") {
                //this._userService.refreshUser();
                //}
            }
        });
    }

    reportCampaign() {

        const subject = `Reporting Campaign for ${this.campaign.name}`;
        const body = `Hi!%0D%0A%0D%0AI am reporting the following campaign https://app.upraise.fund/home/${this.campaign.type}/${this.campaign.id} .`;

        window.open(`mailto:support@upraise.fund?subject=${subject}&body=${body}`, '_self');
    }

    followClick() {

        this._detailService
            .followCampaign(this.campaign.id, this.campaign.type, !this.campaign.isUserFollowing)
            .subscribe((resultDTO) => {

                if (resultDTO) {
                    if (resultDTO.status == StatusEnum.Success) {
                        this.campaign.isUserFollowing = resultDTO.data;

                        this.campaign.numberOfFollowers = this.campaign.numberOfFollowers + (resultDTO.data ? 1 : -1);
                        this._changeDetectorRef.markForCheck();
                    }
                }

            });
    }
    
    public manageCampaign(): void {
        if (this.campaign)
            this._router.navigate(['/funding/new-campaign', this.campaign.transactionId]);
    }

    public closeCampaignPreview(): void {

        if (this.dialogRef)
            this.dialogRef.close();
    }

}
