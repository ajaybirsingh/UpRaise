import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSelectChange } from '@angular/material/select';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { BehaviorSubject, combineLatest, fromEvent, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, switchMap, takeUntil, tap } from 'rxjs/operators';
import { HomeService } from '../home.service';
import { IPublicCampaign } from '../home.types';
import { EnumDTO } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { CampaignTypes, CampaignViewTypes } from '../../../../shared/enums';
import { catchError, map } from 'rxjs/operators';
import { HttpClient, HttpBackend } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { IPublicClosestLocationDTO } from './explore-map.types';


@Component({
    selector: 'explore',
    templateUrl: './explore.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExploreComponent implements OnInit, OnDestroy {

    @ViewChild('query', { static: true }) query: ElementRef;

    public CampaignViewTypes = CampaignViewTypes;
    public CampaignTypes = CampaignTypes;

    public campaignViewType: CampaignViewTypes = CampaignViewTypes.List;
    publicClosestLocationDTO: IPublicClosestLocationDTO;
    isSignedIn: boolean;

    categories: EnumDTO[];
    campaigns: IPublicCampaign[];
    filters: {
        campaignType$: BehaviorSubject<number>;
        categoryValue$: BehaviorSubject<number>;
        query$: BehaviorSubject<string>;
        hideCompleted$: BehaviorSubject<boolean>;
        mapBounds$: BehaviorSubject<number[]>;
    } = {
            campaignType$: new BehaviorSubject(CampaignTypes.Any),
            categoryValue$: new BehaviorSubject(0),
            query$: new BehaviorSubject(''),
            hideCompleted$: new BehaviorSubject(false),
            mapBounds$: new BehaviorSubject([])
        };

    private _unsubscribeAll: Subject<any> = new Subject<any>();
    apiLoaded: boolean;
    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _router: Router,
        private _homeService: HomeService,
        private _authService: AuthService,
        private _enumsService: EnumsService,
        private httpClient: HttpClient
    ) {



    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        this.isSignedIn = this._activatedRoute.snapshot.data['isSignedIn'];

        this.apiLoaded = this._activatedRoute.snapshot.data['apiLoaded'];

        this.publicClosestLocationDTO = this._activatedRoute.snapshot.data['closestLocation'];

        //this._activatedRoute.data.subscribe(data => {
            //console.log('Check route resolver data')
            //console.log(data)
        //})


        this._homeService.categories$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((categories: EnumDTO[]) => {
                this.categories = categories;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        // Get the courses
        this._homeService.campaigns$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((campaigns: IPublicCampaign[]) => {
                this.campaigns = campaigns;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            },
                error => console.log(error));

        // Filter the courses

        combineLatest([this.filters.campaignType$, this.filters.categoryValue$, this.filters.query$, this.filters.hideCompleted$, this.filters.mapBounds$])
            .subscribe(([campaignType, categoryValue, query, hideCompleted, mapBounds]) => {

                //console.log('combineLatest');
                              

                let data = {
                    NumberOfCampaigns: 12,
                    FilterCampaignType: campaignType,
                    FilterCategoryId: categoryValue,
                    FilterTitleOrDescription: query,
                    FilterHideCompleted: hideCompleted,
                    ExploreViewType: this.campaignViewType,
                    MapBounds: mapBounds
                };

                this.campaigns = null;
                this._homeService.getPublicCampaigns(data).subscribe();

            });


        fromEvent(this.query.nativeElement, 'keyup')
            .pipe(
                filter(Boolean),
                debounceTime(400),
                distinctUntilChanged(),
                tap((text) => {
                    this.filters.query$.next(this.query.nativeElement.value);
                    //console.log(this.query.nativeElement.value)
                })
            )
            .subscribe();
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
     * Filter by search query
     *
     * @param query
     */
    //filterByQuery(query: string): void {
    //this.filters.query$.next(query);
    //}

    filterByCampaignType(change: MatSelectChange): void {
        this.filters.campaignType$.next(change.value);
    }

    /**
     * Filter by category
     *
     * @param change
     */
    filterByCategory(change: MatSelectChange): void {
        this.filters.categoryValue$.next(change.value);
    }

    /**
     * Show/hide completed courses
     *
     * @param change
     */
    toggleCompleted(change: MatSlideToggleChange): void {
        this.filters.hideCompleted$.next(change.checked);
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




    changeToViewList() {
        this.campaignViewType = CampaignViewTypes.List;

        this._changeDetectorRef.markForCheck();
    }

    changeToViewMap() {
        this.campaignViewType = CampaignViewTypes.Map;

        this._changeDetectorRef.markForCheck();
    }


    public mapBoundsChanged(mapBounds: google.maps.LatLngBounds) {
        //console.log('Map Bound Changed')

        const extendedMapBounds: google.maps.LatLngBounds = mapBounds;

        var bounds: number[] = [];
        if (extendedMapBounds) {
            bounds.push(extendedMapBounds.getSouthWest().lat());
            bounds.push(extendedMapBounds.getSouthWest().lng());
            bounds.push(extendedMapBounds.getNorthEast().lat());
            bounds.push(extendedMapBounds.getNorthEast().lng());
        }

        this.filters.mapBounds$.next(bounds);
    }

}
