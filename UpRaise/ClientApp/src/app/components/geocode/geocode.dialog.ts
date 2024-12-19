import { Component, Inject, ChangeDetectorRef, OnDestroy, OnInit, AfterViewInit, ViewEncapsulation, ElementRef } from '@angular/core';
import { StatusEnum } from '../../models';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { fuseAnimations } from '@fuse/animations';
//import { ProfileService } from 'app/main/pages/profile/profile.service';
import { ViewChild } from '@angular/core';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ImageCropperModule } from 'ngx-image-cropper';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
//import { NgxUiLoaderService } from 'ngx-ui-loader';
import { IUser } from 'app/models/user';
import { UserService } from 'app/core/user/user.service';
import { FuseUtilsService } from '@fuse/services/utils/utils.service';
import { NewCampaignService } from '../../modules/admin/funding/new-campaign/new-campaign.service';
import { GoogleMap } from '@angular/google-maps';
import { ActivatedRoute } from '@angular/router';
import { ResultDTO } from '../../models';
import { IGeocodingResponse } from '../../modules/admin/funding/new-campaign/new-campaign.types';

export interface DialogData {
    formattedAddress: string;
    latitude: number;
    longitude: number;
}


@Component({
    selector: 'geocode',
    templateUrl: './geocode.dialog.html',
    styleUrls: ['./geocode.dialog.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class GeocodeDialog implements OnInit, OnDestroy, AfterViewInit {
    private subUser: any;
    private currentUser: IUser;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    @ViewChild(GoogleMap, { static: false }) map: GoogleMap;
    @ViewChild('mapDiv', { static: false }) mapDiv: any;
    @ViewChild('geoLocation') private _geoLocation: ElementRef;
    markers = []
    apiLoaded: boolean;
    zoom;
    center: google.maps.LatLngLiteral;

    geocodeResponse: IGeocodingResponse = null;

    //https://developers.google.com/maps/documentation/javascript/reference/map
    options: google.maps.MapOptions = {
        zoomControl: true,
        scrollwheel: true,
        disableDoubleClickZoom: false,
        fullscreenControl: false,
        streetViewControl: false,
        mapTypeId: 'hybrid',
        maxZoom: 18,
        minZoom: 4,
    };

    constructor(
        //private ngxService: NgxUiLoaderService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        public matDialogRef: MatDialogRef<GeocodeDialog>,
        private _userService: UserService,
        private _fuseUtilsService: FuseUtilsService,
        private _newCampaignService: NewCampaignService,
        @Inject(MAT_DIALOG_DATA) public data: DialogData
    ) {
    }

    public ngOnInit(): void {

        // Subscribe to user changes
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.currentUser = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });
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
    public ngAfterViewInit() {

        if (!this.data.formattedAddress ||
            !this.data.latitude ||
            !this.data.longitude) {
            this.centerOnUS();
        }
        else {
            this.setLocationData(this.data.formattedAddress, this.data.latitude, this.data.longitude);
        }

    }

    public ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    public save(): void {
        this.matDialogRef.close(this.geocodeResponse);
    }

    private setLocationData(formattedAddres: string, latitude: number, longitude: number) {

        this._geoLocation.nativeElement.value = formattedAddres;
        this.markers = [];
        this.markers.push({
            position: {
                lat: latitude,
                lng: longitude,
            },
            options: {
                animation: google.maps.Animation.BOUNCE,
            },
        });

        this.zoom = 15;
        this.center = {
            lat: latitude,
            lng: longitude,
        };

    }

    public geocode() {
        //const countryId = this.stepData.get('geoLocationCountryId').value;
        const location = this._geoLocation.nativeElement.value;
        if (location) {
            this._newCampaignService.geocode(location).subscribe(
                resultDTO => {

                    this.geocodeResponse = null;

                    if (resultDTO.status === StatusEnum.Success) {
                        this.geocodeResponse = resultDTO.data;
                        this.setLocationData(this.geocodeResponse.formattedAddress, this.geocodeResponse.latitude, this.geocodeResponse.longitude);
                    }

                }
            );
        }

    }

    public locationKeyPress(event: any) {
        this.geocode();
    }
}
