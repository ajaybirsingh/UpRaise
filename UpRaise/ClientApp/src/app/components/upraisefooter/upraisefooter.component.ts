import { Component, Inject, ChangeDetectorRef, OnDestroy, OnInit, AfterViewInit, ViewEncapsulation, ElementRef } from '@angular/core';
import swal, { SweetAlertOptions } from 'sweetalert2';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { fuseAnimations } from '@fuse/animations';
import { ViewChild } from '@angular/core';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { IUser } from 'app/models/user';
import { UserService } from 'app/core/user/user.service';
import { FuseUtilsService } from '@fuse/services/utils/utils.service';
import { FormControl, Validators, FormBuilder, FormGroup, ValidatorFn, ValidationErrors, AbstractControl } from '@angular/forms';
import { StatusEnum } from '../../models';
import { UpraiseFooterService } from './upraisefooter.service';
import { ChangeDetectionStrategy } from '@angular/core';


@Component({
    selector: 'upraisefooter',
    templateUrl: './upraisefooter.component.html',
    styleUrls: ['./upraisefooter.component.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpraiseFooterComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild('newsletterEmailAddress', { static: true }) newsletterEmailAddress: ElementRef;

    private subUser: any;
    private currentUser: IUser;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    public isSignedIn = false;
    public showSubscriptionAlert: boolean = false;

    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private _fuseUtilsService: FuseUtilsService,
        private _upraiseFooterService: UpraiseFooterService
    ) {

    }

    public ngOnInit(): void {
    }


    public ngAfterViewInit() {
    }

    public ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }


    get currentYear(): number {
        return new Date().getFullYear();
    }

    public newsletterSignUp(): void {

        const emailAddress = this.newsletterEmailAddress.nativeElement.value;
        if (emailAddress) {
            this._upraiseFooterService
                .newsletter(emailAddress)
                .subscribe((resultDTO) => {

                    if (resultDTO) {
                        if (resultDTO.status == StatusEnum.Success) {

                            this.newsletterEmailAddress.nativeElement.value = '';

                            this.showSubscriptionAlert = true;
                            this._changeDetectorRef.markForCheck();
                        }
                    }

                });
        }

    }

    public hideSubscriptionAlert() {
        this.showSubscriptionAlert = false;
    }

        
}
