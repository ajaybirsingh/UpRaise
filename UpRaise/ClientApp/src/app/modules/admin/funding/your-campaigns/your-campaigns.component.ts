import { ChangeDetectionStrategy, Component, OnInit, ViewEncapsulation, ViewChild, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CampaignService } from '../../../../core/campaigns/campaigns.service';
import { IYourCampaignResponseDTO, YourCampaignSortOrders } from './your-campaigns.types';
import { BehaviorSubject, combineLatest, fromEvent, Subject } from 'rxjs';
import { YourCampaignsService } from './your-campaigns.service.';
import { takeUntil } from 'rxjs/operators';
import { CampaignStatuses, CampaignTypes } from '../../../../shared/enums';
import { MatSelectChange } from '@angular/material/select';
import { Router } from '@angular/router';


@Component({
    selector: 'your-campaigns',
    templateUrl: './your-campaigns.component.html',
    encapsulation: ViewEncapsulation.None,
    styleUrls: ['./your-campaigns.styles.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class YourCampaignsComponent implements OnInit, OnDestroy {

    public yourCampaignResponse: IYourCampaignResponseDTO = null;

    public CampaignTypes = CampaignTypes;
    public CampaignStatuses = CampaignStatuses;

    public YourCampaignSortOrders = YourCampaignSortOrders;

    filters: {
        pageNumber$: BehaviorSubject<number>;
        pageSize$: BehaviorSubject<number>;
        sortOrder$: BehaviorSubject<YourCampaignSortOrders>;
    } = {
            pageNumber$: new BehaviorSubject(1),
            pageSize$: new BehaviorSubject(20),
            sortOrder$: new BehaviorSubject(YourCampaignSortOrders.Newest),
        };

    private _unsubscribeAll: Subject<any> = new Subject<any>();


    //sortOrder: YourCampaignSortOrders;
    /**
     * Constructor
     */
    constructor(private _yourCampaignsService: YourCampaignsService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _router: Router) {
        //this._campaignService.getYourCampaigns(this.state)
        //.subscribe(c => this.campaigns = c);
    }


    ngOnInit(): void {

        // Get the courses
        this._yourCampaignsService.yourCampaigns$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((yourCampaignResponse: IYourCampaignResponseDTO) => {
                this.yourCampaignResponse = yourCampaignResponse;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            },
                error => console.log(error));


        combineLatest([this.filters.pageNumber$, this.filters.pageSize$, this.filters.sortOrder$])
            .subscribe(([pageNumber, pageSize, sortOrder]) => {

                //console.log('combineLatest');

                let data = {
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    sortOrder: sortOrder
                };

                //this.campaigns = null;
                this._yourCampaignsService.getYourCampaigns(data).subscribe();
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

    previousPage(): void {
        const currentPage = this.filters.pageNumber$.value;

        if (currentPage > 1)
            this.filters.pageNumber$.next(currentPage - 1);
    }

    nextPage(): void {
        const currentPage = this.filters.pageNumber$.value;

        if (currentPage < this.yourCampaignResponse.totalPages)
            this.filters.pageNumber$.next(currentPage + 1);
    }

    public edit(campaignTxId: string) {
        this._router.navigate(['/funding/new-campaign', campaignTxId]);
    }

    public view(typeId: number, campaignId: number) {
        //this._router.navigate(['/home/detail', { type: typeId, id: campaignId }]);
        this._router.navigate(['/home/detail', typeId, campaignId ]);
    }


    public filterBySortOrder(change: MatSelectChange): void {
        this.filters.sortOrder$.next(change.value);
    }


}

