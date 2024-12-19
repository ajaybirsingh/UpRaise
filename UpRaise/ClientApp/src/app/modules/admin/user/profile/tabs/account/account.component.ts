import { Component, OnDestroy, OnInit, ViewEncapsulation, ViewChild, ChangeDetectorRef, ElementRef, Renderer2, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { fuseAnimations } from '@fuse/animations';
//import { ProfileService } from 'app/main/pages/profile/profile.service';
import swal from 'sweetalert2';
import { ImageSelectorDialog } from 'app/components/image-selector/image-selector.dialog';
import { FormControl, Validators, FormBuilder, FormGroup, ValidatorFn, ValidationErrors, AbstractControl } from '@angular/forms';
import { TwoFactorUserSecurityDTO, StatusEnum } from 'app/models';
import { IUser, IUserNotifications, IUserPersonalInformation } from 'app/models/user';
import { AuthService } from 'app/core/auth/auth.service';
import { TwoFactorAuthService } from 'app/core/2fa/two-factor-auth.service';
import { UserService } from 'app/core/user/user.service';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { TwoFactorAuthAuthenticatorDetailDTO, ResultDTO } from 'app/models';
import { FuseAlertType } from '@fuse/components/alert';


@Component({
    selector: 'account',
    templateUrl: './account.component.html',
    //styleUrls: ['./account.component.css'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})

export class AccountComponent implements OnInit, OnDestroy {
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    @ViewChild('fileInput') fileInput: ElementRef;
    about: any;
    public currentUser: IUser;

    public personalInformationFormGroup = new FormGroup({
        'firstName': new FormControl('', Validators.required),
        'lastName': new FormControl('', Validators.required),
        'country': new FormControl(''),
        'streetAddress': new FormControl(''),
        'city': new FormControl(''),
        'stateProvince': new FormControl(''),
        'zipPostal': new FormControl(''),
        'defaultCurrencyId': new FormControl(Validators.required),
    });

    /*
     * 'firstName': new FormControl(dataItem.firstName, Validators.required),
            'lastName': new FormControl(dataItem.lastName, Validators.required),
                'country': new FormControl(dataItem.country),
                    'streetAddress': new FormControl(dataItem.streetAddress),
                        'city': new FormControl(dataItem.city),
                            'stateProvince': new FormControl(dataItem.stateProvince),
                                'zipPostal': new FormControl(dataItem.zipPostal),*/


    public notificationsFormGroup = new FormGroup({
        'notificationOnCampaignDonations': new FormControl(false),
        'notificationOnCampaignFollows': new FormControl(false),
        'notificationOnUpraiseEvents': new FormControl(false)
    });



    //public displayAuthenticator: boolean = false;


    /**
     * Constructor
     *
     * @param {ProfileService} _profileService
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private renderer2: Renderer2,
        @Inject(DOCUMENT) private _document,
        private dialog: MatDialog,
        private _authenticationService: AuthService,
        private _userService: UserService,

        //private _profileService: ProfileService
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {


        // Subscribe to user changes
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {

                this.currentUser = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        this._userService.getNotifications()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((resultDTO: ResultDTO) => {

                if (resultDTO.status === StatusEnum.Success) {

                    const userNotifications: IUserNotifications = resultDTO.data;

                    this.notificationsFormGroup.reset({
                        'notificationOnCampaignDonations': userNotifications.notificationOnCampaignDonations,
                        'notificationOnCampaignFollows': userNotifications.notificationOnCampaignFollows,
                        'notificationOnUpraiseEvents': userNotifications.notificationOnUpraiseEvents
                    });
                }

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        this._userService.getPersonalInformation()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((resultDTO: ResultDTO) => {

                if (resultDTO.status === StatusEnum.Success) {

                    const userPersonalInformation: IUserPersonalInformation = resultDTO.data;

                    this.personalInformationFormGroup.reset({
                        'firstName': userPersonalInformation.firstName,
                        'lastName': userPersonalInformation.lastName,
                        'country': userPersonalInformation.country,
                        'streetAddress': userPersonalInformation.streetAddress,
                        'city': userPersonalInformation.city,
                        'stateProvince': userPersonalInformation.stateProvince,
                        'zipPostal': userPersonalInformation.zipPostal,
                        'defaultCurrencyId': userPersonalInformation.defaultCurrencyId,
                    });
                }

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        //.add(() => console.log('Unsubscribed'));;

    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }







    selectProfilePicture(): void {
        this.fileInput.nativeElement.value = '';
        this.fileInput.nativeElement.click();
    }


    public fileChangeEvent(event: any): void {

        const dialogRef = this.dialog.open(ImageSelectorDialog, {
            //panelClass: 'mail-ngrx-compose-dialog'
            data: { saveType: 2, roundCropper: true, imageChangedEvent: event, fileName: event.target.files[0].name }
        });

        dialogRef.afterClosed().subscribe(result => {

            if (result) {
                if (result.Result.toLowerCase() === "save") {
                    this._userService.refreshUser();
                }
            }
        });

    }


    public savePersonalInformation() {
        for (var i in this.personalInformationFormGroup.controls) {
            this.personalInformationFormGroup.controls[i].markAsTouched();
        }

        if (this.personalInformationFormGroup.invalid) {
            return;
        }

        this.personalInformationFormGroup.disable();

        const userPersonalInformation: IUserPersonalInformation = this.personalInformationFormGroup.value;

        this._userService.updatePersonalInformation(userPersonalInformation).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success) {
                    this._userService.refreshUser();
                    swal.fire('Updated', "Personal information are updated.", 'success');
                }
                else {
                    swal.fire('Update Error', "Unable to update personal information.", 'error');
                }
            },
            error => {
                console.error(error);
                this.personalInformationFormGroup.enable();
            },
            () => {
                this.personalInformationFormGroup.enable();
            });
    }



    public saveNotifications() {
        for (var i in this.notificationsFormGroup.controls) {
            this.notificationsFormGroup.controls[i].markAsTouched();
        }

        if (this.notificationsFormGroup.invalid) {
            return;
        }

        this.notificationsFormGroup.disable();

        const userNotifications: IUserNotifications = this.notificationsFormGroup.value;

        this._userService.updateNotifications(userNotifications).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success)
                    swal.fire('Updated', "Notifications are updated.", 'success');
                else {
                    swal.fire('Update Error', "Unable to update notifications.", 'error');
                }
            },
            error => {
                console.error(error);
                this.notificationsFormGroup.enable();
            },
            () => {
                this.notificationsFormGroup.enable();
            });
    }

}

