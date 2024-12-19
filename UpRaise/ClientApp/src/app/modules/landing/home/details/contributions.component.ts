import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import * as moment from 'moment';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { BehaviorSubject, combineLatest, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { IPublicCampaignDetail } from '../home.types';
import { HomeService } from '../home.service';
import { EnumDTO } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { ActivatedRoute } from '@angular/router';
import { DetailService } from './details.service';
import { CampaignAnalyticTypes } from './details.enums';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../../core/user/user.service';
import { ContributionSortFields, IPublicCampaignContributionRequestDTO, IPublicCampaignContributionResponseDTO, SortDirections } from "./contributions.types"
import { ContributionService } from './contributions.service';

@Component({
    selector: 'contributions',
    templateUrl: './contributions.component.html',
    styleUrls: ['./contributions.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContributionsComponent implements OnInit, OnDestroy {

    @Input() campaign: IPublicCampaignDetail; // here is the data variable which we are getting from the parent

    contributionResponse: IPublicCampaignContributionResponseDTO; // here is the data variable which we are getting from the parent

    SortFields = ContributionSortFields;
    SortDirections = SortDirections;

    filters: {
        pageNumber$: BehaviorSubject<number>;
        pageSize$: BehaviorSubject<number>;
        sortField$: BehaviorSubject<ContributionSortFields>;
        sortDirection$: BehaviorSubject<SortDirections>;
    } = {
            pageNumber$: new BehaviorSubject(1),
            pageSize$: new BehaviorSubject(12),
            sortField$: new BehaviorSubject(ContributionSortFields.Date),
            sortDirection$: new BehaviorSubject(SortDirections.Descending),
        };

    private _unsubscribeAll: Subject<any> = new Subject<any>();


    /**
     * Constructor
     */
    constructor(
        private _contributionService: ContributionService,
        private _userService: UserService,
        private _changeDetectorRef: ChangeDetectorRef,
    ) {



        this._contributionService.contributions$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((contributionResponse: IPublicCampaignContributionResponseDTO) => {
                this.contributionResponse = contributionResponse;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            },
                error => console.log(error));


    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        combineLatest([this.filters.pageNumber$, this.filters.pageSize$, this.filters.sortField$, this.filters.sortDirection$])
            .subscribe(([pageNumber, pageSize, sortField, sortDirection]) => {

                let data: IPublicCampaignContributionRequestDTO = {
                    campaignId: this.campaign.id,
                    campaignType: this.campaign.type,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    sortField: sortField,
                    sortDirection: sortDirection,
                };

                this.contributionResponse = null;
                this._contributionService
                    .getPublicCampaignContributions(data)
                    .subscribe();

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

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    changeContributionSorting() {
        const newSortDirection = (this.filters.sortDirection$.value == SortDirections.Ascending) ? SortDirections.Descending : SortDirections.Ascending;
        this.filters.sortDirection$.next(newSortDirection);
    }

    orderContributionByDate() {
        if (this.filters.sortField$.value != this.SortFields.Date) 
            this.filters.sortField$.next(this.SortFields.Date);
    }

    orderContributionByTopContribution() {
        if (this.filters.sortField$.value != this.SortFields.TopContributions)
            this.filters.sortField$.next(this.SortFields.TopContributions);
    }


    getRelativeFormat(date: string): string {
        const today = moment().startOf('day');
        const yesterday = moment().subtract(1, 'day').startOf('day');

        // Is today?
        if (moment(date, moment.ISO_8601).isSame(today, 'day')) {
            return 'Today';
        }

        // Is yesterday?
        if (moment(date, moment.ISO_8601).isSame(yesterday, 'day')) {
            return 'Yesterday';
        }

        return moment(date, moment.ISO_8601).fromNow();
    }




}



