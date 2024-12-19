import { Component, Inject, OnDestroy, OnInit, Renderer2, ViewEncapsulation } from '@angular/core';
import swal from 'sweetalert2';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { fuseAnimations } from '@fuse/animations';
import { TwoFactorAuthService } from '../../../../../../core/2fa/two-factor-auth.service';
import { AbstractControl, FormControl, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { StatusEnum, TwoFactorAuthAuthenticatorDetailDTO, TwoFactorUserSecurityDTO } from '../../../../../../models';
import { DOCUMENT } from '@angular/common';
import { FuseAlertType } from '@fuse/components/alert';
import { UserService } from '../../../../../../core/user/user.service';

declare var QRCode: any;

const changePasswordFormGroup = (dataItem) => new FormGroup({
    'oldPassword': new FormControl(dataItem.oldPassword, Validators.required),
    'newPassword': new FormControl(dataItem.newPassword, Validators.required),
    'reenterNewPassword': new FormControl(dataItem.reenterNewPassword, [Validators.required, confirmPasswordValidator])
});

@Component({
    selector     : 'password',
    templateUrl: './password.component.html',
    styleUrls: ['./password.component.css'],
    encapsulation: ViewEncapsulation.None,
    animations   : fuseAnimations
})
export class PasswordComponent implements OnInit, OnDestroy
{

    public changePasswordFormGroup: FormGroup;

    //2fa
    public twoFactorAuthAuthenticatorDetailDTO: TwoFactorAuthAuthenticatorDetailDTO;
    public twoFactorDisplayAuthenticator: boolean = false;
    public twoFactorGeneratingQrCode: boolean = false;
    public twoFactorVerificationCode: string = '';
    public twoFactorGeneratedQRCode: any;
    public twoFactorRecoveryCodes: string[] = [];
    public twoFactorErrors: string = '';
    public twoFactorUserSecurityDTO: TwoFactorUserSecurityDTO = null;



    // Private
    private _unsubscribeAll: Subject<any>;

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    showAlert: boolean = false;

    /**
     * Constructor
     *
     * @param {ProfileService} _profileService
     */
    constructor(
        private renderer2: Renderer2,
        @Inject(DOCUMENT) private _document,
        private _userService: UserService,
        private _twoFactorAuthService: TwoFactorAuthService
    )
    {
        // Set the private defaults
        this._unsubscribeAll = new Subject();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        const s = this.renderer2.createElement('script');
        s.type = 'text/javascript';
        s.src = 'assets/js/qrcode.min.js';
        s.text = ``;
        this.renderer2.appendChild(this._document.body, s);


        this.twoFactorAuthAuthenticatorDetailDTO = new TwoFactorAuthAuthenticatorDetailDTO();
        //this.twoFactorUserSecurityDTO = new TwoFactorUserSecurityDTO();

        this.changePasswordFormGroup = changePasswordFormGroup({
            'oldPassword': '',
            'newPassword': '',
            'reenterNewPassword': ''
        });

        this._twoFactorAuthService.getDetails().subscribe(
            x => {
                this.twoFactorUserSecurityDTO = x
            }, error => {
                console.error(error);
            }
        );
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    public changePassword() {
        // Return if the form is invalid

        for (var i in this.changePasswordFormGroup.controls) {
            this.changePasswordFormGroup.controls[i].markAsTouched();
        }

        if (this.changePasswordFormGroup.invalid) {
            return;
        }

        this.changePasswordFormGroup.disable();

        this.showAlert = false;

        this._userService.resetPassword(this.changePasswordFormGroup.value).subscribe(
            resultDTO => {
                if (resultDTO.status === StatusEnum.Success)
                    swal.fire('Reset Password', "Password has been updated.", 'success');
                else {
                    var errorMessage = '';

                    if (resultDTO.data && resultDTO.data.length) {
                        errorMessage = "There was an error resetting your password.\nPlease fix the following and retry: \n";
                        for (var i = 0; i < resultDTO.data.length; i++) {
                            errorMessage += '- ' + resultDTO.data[i] + '\n';
                        }
                    }
                    else
                        errorMessage = resultDTO.message;

                    // Set the alert
                    this.alert = {
                        type: 'error',
                        message: errorMessage
                    };

                    // Show the alert
                    this.showAlert = true;

                    swal.fire('Reset Password', "Password has not been updated.", 'error');
                }
            },
            error => {
                console.error(error);
                this.changePasswordFormGroup.enable();
            },
            () => {
                this.changePasswordFormGroup.enable();
            });
    }


    public verifyAuthenticator() {
        var verification = {
            verificationCode: this.twoFactorVerificationCode.toString()
        };

        this.twoFactorErrors = '';

        this._twoFactorAuthService.verifyAuthenticator(verification).subscribe(
            x => {
                let verifyAuthenticatorResult = x;
                if (verifyAuthenticatorResult.status === StatusEnum.Success) {

                    swal.fire('2fa', verifyAuthenticatorResult.message, 'success');

                    if (verifyAuthenticatorResult.data && verifyAuthenticatorResult.data.recoveryCodes) {
                        // display new recovery codes
                        this.twoFactorRecoveryCodes = verifyAuthenticatorResult.data.recoveryCodes;
                    }

                    this.twoFactorDisplayAuthenticator = false;
                    this.twoFactorGeneratingQrCode = false;
                    this.twoFactorUserSecurityDTO.hasAuthenticator = true;
                    this.twoFactorUserSecurityDTO.twoFactorEnabled = true;
                }
                else
                    if (verifyAuthenticatorResult.status === StatusEnum.Error) {
                        this.twoFactorErrors = verifyAuthenticatorResult.data.toString();
                    }
            },
            error => console.error(error)
        );

    }

    public onKeydown(event: any) {
        if (event.key === "Enter") {
            this.verifyAuthenticator();
        }
    }

    public resetRecoverCodes() {

        this._twoFactorAuthService.generateRecoveryCodes().subscribe(
            x => {
                let generateRecoverCodesResult = x;

                if (generateRecoverCodesResult.status === StatusEnum.Success) {

                    swal.fire('Reset 2FA Recovery Codes', generateRecoverCodesResult.message, 'success');

                    if (generateRecoverCodesResult.data && generateRecoverCodesResult.data.recoveryCodes) {
                        // display new recovery codes
                        this.twoFactorRecoveryCodes = generateRecoverCodesResult.data.recoveryCodes;
                    }
                } else {
                    swal.fire('Reset 2FA Recovery Codes', generateRecoverCodesResult.message, 'error');
                }
            },
            error => console.error(error));
    }

    public setupAuthenticator() {
        let self = this;
        self.twoFactorRecoveryCodes = [];

        this._twoFactorAuthService.setupAuthenticator().subscribe(
            x => {
                this.twoFactorAuthAuthenticatorDetailDTO = x;
                console.log(this.twoFactorAuthAuthenticatorDetailDTO);
                this.twoFactorDisplayAuthenticator = true;
                this.twoFactorGeneratingQrCode = true;
                setTimeout(function () {
                    self.twoFactorGeneratedQRCode = new QRCode(document.getElementById("genQrCode"),
                        {
                            text: self.twoFactorAuthAuthenticatorDetailDTO.authenticatorUri,
                            width: 150,
                            height: 150,
                            colorDark: "#000",
                            colorLight: "#ffffff",
                            correctLevel: QRCode.CorrectLevel.H
                        });
                    self.twoFactorGeneratingQrCode = false;
                    (document.querySelector("#genQrCode > img") as HTMLInputElement).style.margin = "0 auto";
                },
                    200);
            },
            error => console.error(error)
        );
    }



    public disable2FA() {
        this._twoFactorAuthService.disable2FA().subscribe(
            x => {
                let disable2FAResult = x;

                if (disable2FAResult.status === StatusEnum.Success) {
                    swal.fire('2fa', disable2FAResult.message, 'success');

                    this.twoFactorUserSecurityDTO.twoFactorEnabled = false;
                    this.twoFactorRecoveryCodes = [];
                }
                else {
                    swal.fire('2fa', disable2FAResult.message, 'error');
                }
            },
            error => console.error(error)
        );
    }


}



/**
 * Confirm password validator
 *
 * @param {AbstractControl} control
 * @returns {ValidationErrors | null}
 */
export const confirmPasswordValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {

    if (!control.parent || !control) {
        return null;
    }

    const password = control.parent.get('newPassword');
    const passwordConfirm = control.parent.get('reenterNewPassword');

    if (!password || !passwordConfirm) {
        return null;
    }

    if (passwordConfirm.value === '') {
        return null;
    }

    if (password.value === passwordConfirm.value) {
        return null;
    }

    return { 'passwordsNotMatching': true };
};
