import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { AbstractControl, FormControl, FormBuilder, FormGroup, NgForm, Validators, ValidationErrors, ValidatorFn, } from '@angular/forms';
import { Router } from '@angular/router';
import { fuseAnimations } from '@fuse/animations';
import { FuseAlertType } from '@fuse/components/alert';
import { AuthService } from 'app/core/auth/auth.service';
import { takeUntil } from 'rxjs/internal/operators';
import { first } from 'rxjs/operators';
import { StatusEnum } from '../../../models';
//import swal from 'sweetalert2';

@Component({
    selector: 'auth-sign-up',
    templateUrl: './sign-up.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class AuthSignUpComponent implements OnInit {
    @ViewChild('signUpNgForm') signUpNgForm: NgForm;

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    signUpForm: FormGroup;
    showAlert: boolean = false;

    /**
     * Constructor
     */
    constructor(
        private _authService: AuthService,
        private _formBuilder: FormBuilder,
        private _router: Router
    ) {
        /*
    // redirect to home if already logged in
    if (this.authenticationService.currentUserValue) {
      this.router.navigate([DEFAULT_LOGIN_URL]);
    }
         */
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        // Create the form
        this.signUpForm = this._formBuilder.group({
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            username: ['', [Validators.required, Validators.email]],
            password: ['', Validators.required],
            passwordConfirm: ['', [Validators.required, confirmPasswordValidator]],
            agreements: ['', Validators.requiredTrue]
        }
        );

        /*
        this.signUpForm.get('password').valueChanges
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this.registerForm.get('passwordConfirm').updateValueAndValidity();
            });
            */
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Sign up
     */
    signUp(): void {
        // Do nothing if the form is invalid
        if (this.signUpForm.invalid) {
            return;
        }

        // Disable the form
        this.signUpForm.disable();

        // Hide the alert
        this.showAlert = false;

        // Sign up
        this._authService.signUp(this.signUpForm.value)
            .subscribe(

                (response) => {
                    if (response.status === StatusEnum.Success) {

                        // Reset the form
                        this.signUpNgForm.resetForm();
                        this._router.navigateByUrl('/confirmation-required');
                    }
                    else {
                        if (response.status === StatusEnum.Error) {
                            //this.errors = response.data.toString();

                            var errorMessage = '';

                            if (response.data && response.data.length) {
                                errorMessage = "There was an error creating your account.\nPlease fix the following and recreate your account: \n";
                                for (var i = 0; i < response.data.length; i++) {
                                    errorMessage += '- ' + response.data[i] + '\n';
                                }
                            }
                            else
                                errorMessage = response.message;

                            // Set the alert
                            this.alert = {
                                type: 'error',
                                message: errorMessage
                            };

                            // Show the alert
                            this.showAlert = true;

                        }
                    }
                },
                error => {
                    var errorMessage = 'There was an error creating your account.';

                    //if (error.error && error.error.length) {
                    //              errorMessage = "Please fix the following and recreate your account: <br/><br/>";
                    //for (var i = 0; i < error.error.length; i++) {
                    //                errorMessage += '- ' + error.error[i].description + '<br/><br/>';
                    //}
                    //}

                    // Set the alert
                    this.alert = {
                        type: 'error',
                        message: errorMessage
                    };

                    // Show the alert
                    this.showAlert = true;

                    this.signUpForm.enable();
                },
                () => {
                    // Re-enable the form
                    this.signUpForm.enable();
                });

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

    const password = control.parent.get('password');
    const passwordConfirm = control.parent.get('passwordConfirm');

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
