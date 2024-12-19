import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { CampaignRedliningService } from './campaign-redlining.service';
import { ICampaignRedline, ICampaignRedlineComment, ICampaignRedlineCommentRequest, ICampaignRedlineStatusDTO } from './campaign-redlining.types';
import { UserService } from '../../../core/user/user.service';
import { IUser, StatusEnum } from '../../../models';
import { CampaignRedlineEventTypes, CampaignStatuses } from '../../../shared/enums';
import swal, { SweetAlertOptions } from 'sweetalert2';

//https://localhost:5002/campaign-redline/e38bdf40-98b3-4730-857a-9ee438c09a15
//https://localhost:5002/campaign-redline/767b6c71-9097-465e-b499-1da3b6c9c2b1
@Component({
    selector: 'campaign-redlining',
    templateUrl: './campaign-redlining.component.html',
    styleUrls: ['./campaign-redlining.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CampaignRedliningComponent implements OnInit, OnDestroy, AfterViewInit {
    isSignedIn: boolean;

    public CampaignStatuses = CampaignStatuses;
    public CampaignRedlineEventTypes = CampaignRedlineEventTypes;
    public campaignRedline: ICampaignRedline;
    public user: IUser;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    @ViewChild('comment') commentElement; // accessing the reference element

    /**
     * Constructor
     */
    constructor(
        @Inject(DOCUMENT) private _document: Document,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private _campaignRedliningService: CampaignRedliningService,
    ) {

        // Subscribe to user changes
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.user = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        this.campaignRedline = this._activatedRoute.snapshot.data['campaignRedline'];
        this._changeDetectorRef.markForCheck();

    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {

        //this.isSignedIn = this._activatedRoute.snapshot.data['isSignedIn'];
    }

    public ngAfterViewInit(): void {
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

    rejectCampaign() {

        swal.fire({
            html: 'Do you want to reject this campaign?',
            icon: 'question',
            showCancelButton: true
        }
        ).then((result) => {
            if (result.isConfirmed)
                this.updateStatus(false);
        }
        );


    }

    acceptCampaign() {

        swal.fire({
            html: 'Do you want to accept this campaign?',
            icon: 'question',
            showCancelButton: true
        }
        ).then((result) => {
            if (result.isConfirmed)
                this.updateStatus(true);
        }
        );

        
    }

    private updateStatus(approved: boolean) {

        const campaignRedlineStatusDTO: ICampaignRedlineStatusDTO = {
            transactionId: this.campaignRedline.transactionId,
            approved: approved
        };

        this._campaignRedliningService.updateStatus(campaignRedlineStatusDTO).subscribe(
            resultDTO => {

                if (resultDTO.status === StatusEnum.Success) {
                    this.updateRedlineComments();
                }

            },
            error => {
                console.error(error);
            },
            () => {
            }
        );

    }

    updateRedlineComments() {
        this._campaignRedliningService.getCampaignRedlineById(this.campaignRedline.transactionId).subscribe(
            campaignRedlineDTO => {
                this.campaignRedline = campaignRedlineDTO;
                this._changeDetectorRef.markForCheck();
            },
            error => {
                console.error(error);
            },
            () => {
            }
        );
    }
    addComment() {

        const comment = this.commentElement.nativeElement.value;
        if (comment) {

            const campaignRedlineCommentRequest: ICampaignRedlineCommentRequest = {
                campaignId: this.campaignRedline.id,
                comment: comment
            };

            this._campaignRedliningService.addRedlineComment(campaignRedlineCommentRequest).subscribe(
                resultDTO => {

                    if (resultDTO.status === StatusEnum.Success) {
                        this.commentElement.nativeElement.value = '';
                        this._changeDetectorRef.markForCheck();

                        this.updateRedlineComments();
                    }

                },
                error => {
                    console.error(error);
                },
                () => {
                }
            );
        }
    }
}
