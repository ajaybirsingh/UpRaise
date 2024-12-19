import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fuseAnimations } from '@fuse/animations';
import { FuseAlertType } from '@fuse/components/alert';
import { AuthService } from 'app/core/auth/auth.service';
import { TwoFactorAuthService } from 'app/core/2fa/two-factor-auth.service';
import { StatusEnum } from '../../../models';

import { SocialAuthService } from "angularx-social-login";
import { FacebookLoginProvider, GoogleLoginProvider } from "angularx-social-login";
import swal from 'sweetalert2';
import { UserService } from '../../../core/user/user.service';




@Component({
    selector: 'auth-sign-in',
    templateUrl: './sign-in.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class AuthSignInComponent implements OnInit {
    @ViewChild('signInNgForm') signInNgForm: NgForm;
    

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    public signInForm: FormGroup;


    public twoFactorFormGroup = new FormGroup({
        twoFactorCode: new FormControl('', Validators.required),
        rememberMachine: new FormControl(false),
        useRecoveryCode: new FormControl(false),
    });

    showAlert: boolean = false;
    showAlertInvalid2FACode: boolean = false;

    showConfirmationSignIn: boolean = false;

    public twoFactorRequires2FA: boolean = false;



    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _authService: AuthService,
        private _userService: UserService,
        private _twoFactorAuthService: TwoFactorAuthService,
        private _formBuilder: FormBuilder,
        private _router: Router,
        private _socialAuthService: SocialAuthService
    ) {



    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {

        const t = this._activatedRoute.snapshot.queryParamMap.get('t')
        if (t) {
            switch (t) {
                case '1': this.showConfirmationSignIn = true; break;
            }
        }

        // Create the form
        this.signInForm = this._formBuilder.group({
            username: ['', [Validators.required, Validators.email]],
            password: ['', Validators.required],
            rememberMe: ['']
        });


        var rememberMe = localStorage.getItem('rememberCurrentUser') == 'true' ? true : false;
        this.signInForm.controls['rememberMe'].setValue(rememberMe);

        if (this.signInForm.value.rememberMe) {
            var username = localStorage.getItem('currentUsername');
            this.signInForm.controls['username'].setValue(username);
        }


        this._socialAuthService.authState.subscribe((user) => {
            console.log(user.email);
            //this.user = user;
            //this.loggedIn = (user != null);
        });

    }

    private resetTwoFactorFormGroup() {

        this.showAlertInvalid2FACode = false;

        this.twoFactorFormGroup.reset(
            {
                twoFactorCode: '',
                rememberMachine: false,
                useRecoveryCode: false
            });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    
    /**
     * Sign in
     */
    signIn(): void {
        // Return if the form is invalid
        if (this.signInForm.invalid) {
            return;
        }

        this.resetTwoFactorFormGroup();

        // Disable the form
        this.signInForm.disable();

        // Hide the alert
        this.showAlert = false;


        // Sign in
        this._authService.signIn(this.signInForm.value)
            .subscribe((response: any) => {
                if (response.status === StatusEnum.Success) {
                    if (response.data.requires2FA) {
                        //
                        //reset 2fa form
                        //
                        this.twoFactorRequires2FA = true;
                        //this.stateService.displayNotification({ message: loginResult.message, type: "success" });
                        return;
                    }

                    if (this.signInForm.value.rememberMe) {
                        localStorage.setItem('rememberCurrentUser', this.signInForm.value.rememberMe);
                        localStorage.setItem('currentUsername', this.signInForm.value.username);
                    }
                    else {
                        localStorage.removeItem('rememberCurrentUser');
                        localStorage.removeItem('currentUsername');
                    }


                    // Set the redirect url.
                    // The '/signed-in-redirect' is a dummy url to catch the request and redirect the user
                    // to the correct page after a successful sign in. This way, that url can be set via
                    // routing file and we don't have to touch here.

                    const activatedRoute = this._activatedRoute.snapshot.queryParamMap.get('redirectURL');
                    const redirectURL = activatedRoute || '/signed-in-redirect';

                    // Navigate to the redirect url
                    this._router.navigateByUrl(redirectURL);
                }
                else {
                    this.signInForm.enable();

                    // Reset the form
                    this.signInNgForm.resetForm();
                    //this.signInForm.reset();

                    // Set the alert
                    this.alert = {
                        type: 'error',
                        message: 'Wrong email or password'
                    };

                    // Show the alert
                    this.showAlert = true;
                }

            });
    }


    signInGoogle(): void {

        swal.fire(
            'UpRaise Google SSO',
            'We are currently using the Google Sandbox/Test SSO system.  This means your e-mail address needs to be added to our Google SSO whitelist in order to use it here, please contact support.',
            'info'
        ).then(() => {
            //const googleLoginOptions = {
            //scope: 'profile email'
            //offline_access: true
            //}; // https://developers.google.com/api-client-library/javascript/reference/referencedocs#gapiauth2clientconfig

            //, googleLoginOptions


            //this.isLoading = true;

            this.showAlert = false;

            this._socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID)
                .then(userData => {
                    console.log(userData);
                    /*
                    //on success
                    //this will return user data from google. What you need is a user token which you will send it to the server
                    this._authService.googleSignInExternal(userData.idToken)
                        .pipe(finalize(() => this.isLoading = false)).subscribe(result => {
     
                            console.log('externallogin: ' + JSON.stringify(result));
                            if (!(result instanceof SimpleError) && this.credentialsService.isAuthenticated()) {
                                this.router.navigate(['/index']);
                            }
                        });
                      */

                    //const user: SocialUser = { ...res };
                    //console.log(user);
                    //console.log(res);

                }, error => console.log(error));
        }
        );




    }

    public twoFactorPlaceholder() {
        const txt = this.twoFactorFormGroup.value.useRecoveryCode ? 'Enter recovery code' : '6-digit code';
        return txt;
    }

    //
    //Two Factor Authentication Cancel
    //
    public cancel2fa() {
        this.showAlertInvalid2FACode = false;
        this.signInForm.enable();
        this._authService.signOut();
        this.twoFactorRequires2FA = false;
        this.resetTwoFactorFormGroup();

    }

    //
    //Two Factor Authentication Login
    //
    public login2fa() {

        if (this.twoFactorFormGroup.invalid) {
            return;
        }

        // Disable the form
        this.twoFactorFormGroup.disable();

        // Hide the alert
        this.showAlertInvalid2FACode = false;

        let data = {
            username: this.signInForm.value.username,
            password: this.signInForm.value.password,
            twoFactorCode: this.twoFactorFormGroup.value.twoFactorCode,
            useRecoveryCode: this.twoFactorFormGroup.value.useRecoveryCode,
            rememberMachine: this.twoFactorFormGroup.value.rememberMachine
        };

        this._twoFactorAuthService.loginWith2fa(data).subscribe(
            loginResult => {

                this.twoFactorFormGroup.enable();
                // Reset the form
                this.resetTwoFactorFormGroup();

                if (loginResult.status === StatusEnum.Success) {
                    const activatedRoute = this._activatedRoute.snapshot.queryParamMap.get('redirectURL');
                    const redirectURL = activatedRoute || '/signed-in-redirect';

                    // Navigate to the redirect url
                    //this._router.navigateByUrl(redirectURL);
                    this._router.navigate([redirectURL]);

                }
                else {
                    if (loginResult.status === StatusEnum.Error) {
                        this.showAlertInvalid2FACode = true;
                    }
                }

            },
            error => {

                // Set the alert
                this.alert = {
                    type: 'error',
                    message: error.error
                };

                // Show the alert
                this.showAlertInvalid2FACode = true;
            });


    }


}
