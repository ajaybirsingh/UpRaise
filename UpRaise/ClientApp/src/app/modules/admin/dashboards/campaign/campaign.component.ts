import { ChangeDetectorRef, ChangeDetectionStrategy, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ApexOptions } from 'ng-apexcharts';
import { CampaignService } from 'app/modules/admin/dashboards/campaign/campaign.service';
import { IUser } from 'app/models/user';
import { UserService } from 'app/core/user/user.service';
import { IDashboardActivityRequestDTO, IDashboardActivityResponseDTO, IDashboardCampaignRequestDTO, IDashboardCampaignResponseDTO, IDashboardCampaignsResponseDTO } from './campaign.types';
import { CampaignTypes, DateRanges } from '../../../../shared/enums';
import * as moment from 'moment';
import { ResultDTO, StatusEnum } from '../../../../models';
import { Observable } from 'rxjs';

@Component({
    selector: 'campaign',
    templateUrl: './campaign.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CampaignComponent implements OnInit, OnDestroy {

    isBusy: boolean = false;

    chartContributions: ApexOptions = {};

    user: IUser;
    //chartGithubIssues: ApexOptions = {};
    //chartTaskDistribution: ApexOptions = {};
    //chartBudgetDistribution: ApexOptions = {};
    //chartWeeklyExpenses: ApexOptions = {};
    //chartMonthlyExpenses: ApexOptions = {};
    //chartYearlyExpenses: ApexOptions = {};
    data: any;
    campaigns: IDashboardCampaignsResponseDTO[];
    selectedCampaign: IDashboardCampaignsResponseDTO = null;
    selectedCampaignData: IDashboardCampaignResponseDTO = null;
    private _unsubscribeAll: Subject<any> = new Subject<any>();


    //activities$: Observable<IDashboardActivityResponseDTO[]>;
    activities: IDashboardActivityResponseDTO[] = null;


    /**
     * Constructor
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _campaignService: CampaignService,
        private _router: Router,
        private _userService: UserService
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.user = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        this._campaignService.activities$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((response) => {
                this.activities = response;
                this._changeDetectorRef.markForCheck();
            });


        this._campaignService.campaigns$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data) => {
                this.campaigns = data;

                if (this.campaigns && !this.selectedCampaign) {
                    this.selectedCampaign = this.campaigns[0];
                    this.updateSelectedCampaign();
                }
            });

        // Get the data
        this._campaignService.data$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data) => {

                // Store the data
                this.data = data;

                // Prepare the chart data
                this._prepareChartData();
            });

        // Attach SVG fill fixer to all ApexCharts
        window['Apex'] = {
            chart: {
                events: {
                    mounted: (chart: any, options?: any): void => {
                        this._fixSvgFill(chart.el);
                    },
                    updated: (chart: any, options?: any): void => {
                        this._fixSvgFill(chart.el);
                    }
                }
            }
        };
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

    /**
     * Fix the SVG fill references. This fix must be applied to all ApexCharts
     * charts in order to fix 'black color on gradient fills on certain browsers'
     * issue caused by the '<base>' tag.
     *
     * Fix based on https://gist.github.com/Kamshak/c84cdc175209d1a30f711abd6a81d472
     *
     * @param element
     * @private
     */
    private _fixSvgFill(element: Element): void {
        // Current URL
        const currentURL = this._router.url;

        // 1. Find all elements with 'fill' attribute within the element
        // 2. Filter out the ones that doesn't have cross reference so we only left with the ones that use the 'url(#id)' syntax
        // 3. Insert the 'currentURL' at the front of the 'fill' attribute value
        Array.from(element.querySelectorAll('*[fill]'))
            .filter(el => el.getAttribute('fill').indexOf('url(') !== -1)
            .forEach((el) => {
                const attrVal = el.getAttribute('fill');
                el.setAttribute('fill', `url(${currentURL}${attrVal.slice(attrVal.indexOf('#'))}`);
            });
    }

    /**
     * Prepare the chart data from the data
     *
     * @private
     */
    private _prepareChartData(): void {
    }

    private updateSelectedCampaign(): void {

        if (this.selectedCampaign) {

            this.isBusy = true;

            const dashboardRequest: IDashboardCampaignRequestDTO =
            {
                id: this.selectedCampaign.id,
                dateRange: DateRanges.All,
                type: CampaignTypes.Organization,
            }

            this._campaignService
                .getCampaign(dashboardRequest)
                .subscribe((dashboardResponse) => {
                    this.selectedCampaignData = dashboardResponse;
                    this._changeDetectorRef.markForCheck();

                    const dashboardActivityRequestDTO: IDashboardActivityRequestDTO = {
                        campaignId: this.selectedCampaignData.id
                    };
                    this._campaignService.getActivities(dashboardActivityRequestDTO)
                        .subscribe();

                })
                .add(() => this.isBusy = false);
        }


    }

    private changeSelectedCampaign(selectedCampaign: IDashboardCampaignsResponseDTO) {
        this.selectedCampaign = selectedCampaign;
        this.updateSelectedCampaign();
    }


    /**
     * Returns whether the given dates are different days
     *
     * @param current
     * @param compare
     */
    isSameDay(current: string, compare: string): boolean {
        return moment(current, moment.ISO_8601).isSame(moment(compare, moment.ISO_8601), 'day');
    }

    /**
     * Get the relative format of the given date
     *
     * @param date
     */
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
