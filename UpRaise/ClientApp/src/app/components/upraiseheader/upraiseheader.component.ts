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
import { UpraiseHeaderService } from './upraiseheader.service';
import { ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    selector: 'upraiseheader',
    templateUrl: './upraiseheader.component.html',
    styleUrls: ['./upraiseheader.component.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpraiseHeaderComponent implements OnInit, OnDestroy, AfterViewInit {

    private subUser: any;
    private currentUser: IUser;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    public isSignedIn = false;
    public showSubscriptionAlert: boolean = false;

    constructor(
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private _fuseUtilsService: FuseUtilsService,
        private _upraiseHeadearService: UpraiseHeaderService,
        private _authService: AuthService
    ) {

    }

    public ngOnInit(): void {
        this._authService
            .check()
            .pipe(
                takeUntil(this._unsubscribeAll)
            )
            .subscribe((isSignedIn: boolean) => {
                this.isSignedIn = isSignedIn;
            });
    }

    public ngAfterViewInit() {
    }

    public ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

           
}

