import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation, Input, AfterViewInit, SimpleChanges, OnChanges, Output, EventEmitter } from '@angular/core';
import * as moment from 'moment';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { BehaviorSubject, combineLatest, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { IPublicCampaign, IPublicCampaignDetail } from '../home.types';
import { HomeService } from '../home.service';
import { EnumDTO } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../../core/user/user.service';
import { ExploreListService } from './explore-list.service';
import { HttpClient, HttpBackend } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { GoogleMap, MapInfoWindow, MapMarker } from '@angular/google-maps';
import { ExploreMapService } from './explore-map.service';
import { IPublicClosestLocationDTO } from './explore-map.types';

//https://developers.google.com/maps/documentation/javascript/overview
//https://github.com/angular/components/tree/master/src/google-maps#readme
@Component({
    selector: 'explore-map',
    templateUrl: './explore-map.component.html',
    styleUrls: ['./explore-map.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExploreMapComponent implements OnInit, OnDestroy, AfterViewInit {

    @Input() public apiLoaded: boolean;
    @Input() public publicClosestLocationDTO: IPublicClosestLocationDTO;

    @Output() Bounds = new EventEmitter<google.maps.LatLngBounds>();

    @ViewChild(GoogleMap, { static: false }) map: GoogleMap;
    @ViewChild('mapDiv', { static: false }) mapDiv: any;

    @ViewChild(MapInfoWindow, { static: false }) infoWindow: MapInfoWindow
    infoContent = ''
    markers = [];
    campaigns: IPublicCampaign[];

    private _lastBounds: google.maps.LatLngBounds = undefined;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    zoom;
    center: google.maps.LatLngLiteral;
    options: google.maps.MapOptions = {
        zoomControl: true,
        scrollwheel: true,
        disableDoubleClickZoom: false,
        mapTypeId: 'hybrid',
        maxZoom: 18,
        minZoom: 6,
    };


    /**
     * Constructor
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _exploreMapService: ExploreMapService,
        private _homeService: HomeService,
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    private isGeolocationAvailable(): boolean {
        const isAvailable = ((navigator.geolocation !== null) && (navigator.geolocation !== undefined));
        return isAvailable;
    }

    /**
     * On init
     */
    ngOnInit(): void {
        this._homeService.campaigns$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((campaigns: IPublicCampaign[]) => {
                this.campaigns = campaigns;

                this.updateCampaignMarkers();
            },
                error => console.log(error));



    }



    private centerOnUS() {
        var bounds = new google.maps.LatLngBounds();
        bounds.extend(new google.maps.LatLng(24.891752, -98.4375));
        bounds.extend(new google.maps.LatLng(40.351289, -124.244385));
        bounds.extend(new google.maps.LatLng(44.488196, -70.290656));
        bounds.extend(new google.maps.LatLng(49.000282, -101.37085));
        const boundsCenter = bounds.getCenter();
        this.center = {
            lat: boundsCenter.lat(),
            lng: boundsCenter.lng(),
        };

        this.map.fitBounds(bounds);
    }

    ngAfterViewInit() {

        if (!this.isGeolocationAvailable()) {
            if (this.publicClosestLocationDTO) {
                this.center = {
                    lat: this.publicClosestLocationDTO.latitude,
                    lng: this.publicClosestLocationDTO.longitude,
                };
                this.zoom = 11;
                this._changeDetectorRef.markForCheck();
            }
            else
                this.centerOnUS();
        }


        navigator.geolocation.getCurrentPosition(
            (position) => {
                this.center = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude,
                };
                this.zoom = 11;
                this._changeDetectorRef.markForCheck();
            },
            (error) => {

                if (error.code == error.PERMISSION_DENIED) {
                    console.error('User was not given permission');

                    if (this.publicClosestLocationDTO) {
                        this.center = {
                            lat: this.publicClosestLocationDTO.latitude,
                            lng: this.publicClosestLocationDTO.longitude,
                        };
                        this.zoom = 11;
                        this._changeDetectorRef.markForCheck();
                    }
                    else
                        this.centerOnUS();
                }
                else {
                    console.error(error);
                    this.centerOnUS();
                }
            }
        );



        /*
        this.map.boundsChanged
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                const currentBounds = this.map.getBounds();
                if (!this._lastBounds ||
                    !this.isWithinBounds(currentBounds)) {

                    this._lastBounds = currentBounds;
                }

            },
                error => console.log(error));
*/

        this.map.mapDragend
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this.updateBounds();
            },
                error => console.log(error));


        //
        //update current bounds
        //
        this.updateBounds();
    }

    private updateBounds() {
        const currentBounds = this.map.getBounds();
        if (!this._lastBounds ||
            !this.isWithinBounds(currentBounds)) {

            //console.info('mapDragEnd -- new boundaries');
            this.Bounds.emit(currentBounds);

            this._lastBounds = currentBounds;
        }
        else {
            //console.info('mapDragEnd');
        }
    }

    private isWithinBounds(currentBounds: google.maps.LatLngBounds): boolean {

        const isInBounds = false;
        if (this._lastBounds.contains(currentBounds.getNorthEast()) &&
            this._lastBounds.contains(currentBounds.getSouthWest())) {
            return true;
        }

        return isInBounds;
    }


    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    dollarFormatter(dollarValue?: number): string {

        if (!dollarValue)
            return "";

        var formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            // These options are needed to round to whole numbers if that's what you want.
            //minimumFractionDigits: 0, // (this suffices for whole numbers, but will print 2500.10 as $2,500.1)
            //maximumFractionDigits: 0, // (causes 2500.99 to be printed as $2,501)
        });

        var strValue = formatter.format(dollarValue);
        return strValue;

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


    zoomIn() {
        if (this.zoom < this.options.maxZoom) this.zoom++
    }

    zoomOut() {
        if (this.zoom > this.options.minZoom) this.zoom--
    }

    click(event: google.maps.MouseEvent) {
        console.log(event)
    }

    logCenter() {
        console.log(JSON.stringify(this.map.getCenter()))
    }

    addMarker() {

        this.markers.push({
            position: {
                lat: this.center.lat + ((Math.random() - 0.5) * 2) / 10,
                lng: this.center.lng + ((Math.random() - 0.5) * 2) / 10,
            },
            label: {
                color: 'red',
                text: 'Marker label ' + (this.markers.length + 1),
            },
            title: 'Marker title ' + (this.markers.length + 1),
            info: 'Marker info ' + (this.markers.length + 1),
            options: { animation: google.maps.Animation.DROP },
        });

    }

    updateCampaignMarkers() {

        this.markers = [];

        if (this.campaigns) {

            this.campaigns.forEach((campaign, index) => {

                var infoContentHTML: string = '';

                infoContentHTML += '<div>';
                if (campaign.headerPictureURL)
                    infoContentHTML += `<img class="mx-auto h-72" src="${campaign.headerPictureURL}" alt="">`;
                else
                    if (campaign.photos && campaign.photos.length > 0)
                        infoContentHTML += `<img class="mx-auto h-72" src="${campaign.photos[0]}" alt="">`;

                infoContentHTML += '</div>';

                infoContentHTML += '<div class="overflow-hidden h-2 text-xs flex rounded bg-blue-200">';
                infoContentHTML += `<div style="width:${campaign.fundedPercentage}%" class="shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center bg-blue-500"></div>`;
                infoContentHTML += '</div>';

                infoContentHTML += '<div class="px-4 py-2">';

                if (campaign.beneficiary) {
                    if (campaign.typeId == 2)
                        infoContentHTML += `<div class="text-base text-indigo-600 tracking-wide">${campaign.category} ${campaign.beneficiary}</div>`;
                    else {
                        infoContentHTML += `<div class="text-base text-indigo-600 tracking-wide">${campaign.category}</div>`;
                        infoContentHTML += `<div class="text-base text-indigo-600 tracking-wide">${campaign.beneficiary}</div>`;
                    }
                }
                else
                    infoContentHTML += `<div class="text-base text-indigo-600 tracking-wide">${campaign.category}</div>`;

                infoContentHTML += `<h3 class="text-lg leading-6 font-medium text-gray-900">${campaign.name}</h3>`;

                infoContentHTML += `<p class="mt-1 text-sm text-gray-500">${campaign.summary}</p>`;

                infoContentHTML += '</div>';


                //pull to the bottom
                infoContentHTML += '<div class="h-40"><div class="px-4 py-2 absolute bottom-0">';

                infoContentHTML += '<div class="mt-4 grid grid-cols-4 gap-6">';
                infoContentHTML += '<div class="col-span-1">';
                infoContentHTML += '<div class="text-base text-black font-semibold tracking-wide">Raised</div>';
                infoContentHTML += `<div class="text-base text-gray-500 tracking-wide">${campaign.fundedPercentage}%</div>`;
                infoContentHTML += '</div>';

                infoContentHTML += '<div class="col-span-2">';

                if (campaign.fundraisingGoals) {
                    infoContentHTML += '<div class="text-base text-black font-semibold tracking-wide">Goal</div>';
                    infoContentHTML += `<div class="text-base text-gray-500 tracking-wide">USD ${this.dollarFormatter(campaign.fundraisingGoals!)}</div>`;
                }

                infoContentHTML += '</div>';

                infoContentHTML += '<div class="col-span-1">';
                infoContentHTML += '<div class="text-base text-black font-semibold tracking-wide">Donors</div>';
                infoContentHTML += `<div class="text-base text-gray-500 tracking-wide">${campaign.numberOfDonors}</div>`;
                infoContentHTML += '</div>';
                infoContentHTML += '</div>';

                infoContentHTML += '<div class="relative">';
                infoContentHTML += '<div class="absolute inset-0 flex items-center" aria-hidden="true">';
                infoContentHTML += '<div class="w-full border-t border-gray-300"></div>';
                infoContentHTML += '</div>';
                infoContentHTML += '</div>';

                infoContentHTML += '<div class="bg-white px-4 py-5 sm:px-6">';
                infoContentHTML += '<div class="-ml-4 -mt-4 flex justify-between items-center flex-wrap sm:flex-nowrap">';
                infoContentHTML += '<div class="mt-4">';
                infoContentHTML += '<div class="flex items-center">';

                if (campaign.organizedByProfilePictureURL)
                    infoContentHTML += `<div class="flex-shrink-0"><img class="h-12 w-12 rounded-full" src="${campaign.organizedByProfilePictureURL}" alt=""></div>`;
                else
                    infoContentHTML += `<div class="flex-shrink-0"><img class="h-12 w-12 rounded-full" src="assets/images/avatars/user.png" alt=""></div>`;

                infoContentHTML += '<div class="ml-4">';
                infoContentHTML += '<p class="text-sm text-gray-500"><a href="#">Organized By</a></p>';
                infoContentHTML += `<h3 class="text-lg leading-6 font-medium text-gray-900">${campaign.organizedBy}</h3>`;
                infoContentHTML += '</div>';
                infoContentHTML += '</div>';
                infoContentHTML += '</div>';
                infoContentHTML += '<div class="ml-4 mt-4 flex-shrink-0 flex">';

                infoContentHTML += `<button type="button" onclick="location.href='/home/detail/${campaign.typeId}/${campaign.id}'" class="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">`;
                infoContentHTML += '<span>Details</span>';
                infoContentHTML += '<svg xmlns="http://www.w3.org/2000/svg" class="ml-2 h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">';
                infoContentHTML += '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 9l3 3m0 0l-3 3m3-3H8m13 0a9 9 0 11-18 0 9 9 0 0118 0z" />';
                infoContentHTML += '</svg>';
                infoContentHTML += '</button>';

                infoContentHTML += '</div>';
                infoContentHTML += '</div>';
                infoContentHTML += '</div>';

                infoContentHTML += '</div>';
                infoContentHTML += '</div>';

                this.markers.push({
                    position: {
                        lat: campaign.latitude,
                        lng: campaign.longitude,
                    },
                    //label: {
                    //color: 'red',
                    //text: 'Marker label ' + (this.markers.length + 1),
                    //},
                    title: campaign.name,
                    info: infoContentHTML,

                    options: { animation: google.maps.Animation.DROP },
                });


            });

        }

        this._changeDetectorRef.markForCheck();
    }

    openInfo(marker: MapMarker, content) {

        this.infoContent = content;
        this.infoWindow.open(marker);

    }

}



