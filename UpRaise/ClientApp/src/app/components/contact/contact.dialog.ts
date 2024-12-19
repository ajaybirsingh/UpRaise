import { Component, Inject, ChangeDetectorRef, OnDestroy, OnInit, AfterViewInit, ViewEncapsulation } from '@angular/core';
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



export interface ContactDialogData {
    campaignId: number,
    campaignType: number,
    userAliasId: string,
    showProfilePic: boolean,
    fullName: string,
    updatedAt: Date
}

@Component({
    selector: 'contact',
    templateUrl: './contact.dialog.html',
    styleUrls: ['./contact.dialog.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class ContactDialog implements OnInit, OnDestroy, AfterViewInit {
    private subUser: any;
    private currentUser: IUser;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    public formGroup: FormGroup;
    public isSignedIn = false;

    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        public matDialogRef: MatDialogRef<ContactDialog>,
        private _userService: UserService,
        private _fuseUtilsService: FuseUtilsService,
        @Inject(MAT_DIALOG_DATA) public data: ContactDialogData
    ) {


        this.formGroup = new FormGroup({
            'campaignType': new FormControl(data.campaignType, Validators.required),
            'campaignId': new FormControl(data.campaignId, Validators.required),
            'toUserAliasId': new FormControl(data.userAliasId, Validators.required),
            'firstName': new FormControl('', Validators.required),
            'lastName': new FormControl('', Validators.required),
            'email': new FormControl('', [Validators.required, Validators.email]),
            'phone': new FormControl(''),
            'subject': new FormControl('', Validators.required),
            'message': new FormControl('', Validators.required),
        });
    }

    public ngOnInit(): void {
        // Subscribe to user changes
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.currentUser = user;
                // Mark for check

                if (user) {
                    this.isSignedIn = true;
                    this.formGroup.controls['firstName'].clearValidators();
                    this.formGroup.controls['lastName'].clearValidators();
                    this.formGroup.controls['email'].clearValidators();

                    this.formGroup.controls['firstName'].reset();
                    this.formGroup.controls['lastName'].reset();
                    this.formGroup.controls['email'].reset();
                }
                else
                    this.isSignedIn = false;


                this._changeDetectorRef.markForCheck();
            });
    }


    public ngAfterViewInit() {
    }

    public ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }


    public sendMessage(): void {

        for (var i in this.formGroup.controls) {
            this.formGroup.controls[i].markAsTouched();
        }

        if (this.formGroup.invalid)
            return;

        this.formGroup.disable();

        this._userService.sendMessage(this.formGroup.value).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success) {
                    swal.fire('Message', "Message has been sent to user.", 'success');
                    this.matDialogRef.close({ Result: 'sent' })
                }
                else
                    swal.fire('Message', "Error sending message.", 'error');
            },
            error => {
                console.error(error);
                this.formGroup.enable();
            },
            () => {
                this.formGroup.enable();
            });
    }

    public close(): void {
        this.matDialogRef.close({ Result: 'close' })
    }


}
