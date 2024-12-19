import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { fuseAnimations } from '@fuse/animations';
import { FuseValidators } from '@fuse/validators';
import { FuseAlertType } from '@fuse/components/alert';
import { AuthService } from 'app/core/auth/auth.service';
import { StatusEnum } from '../../../models';
import swal  from 'sweetalert2';

@Component({
    selector: 'auth-reset-password',
    templateUrl: './reset-password.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class AuthResetPasswordComponent implements OnInit {
    @ViewChild('resetPasswordNgForm') resetPasswordNgForm: NgForm;

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: ''
    };
    resetPasswordForm: FormGroup;
    showAlert: boolean = false;

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _authService: AuthService,
        private _formBuilder: FormBuilder
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        const username = this._activatedRoute.snapshot.queryParamMap.get('u')
        const resettoken = this._activatedRoute.snapshot.queryParamMap.get('rt')

        // Create the form
        this.resetPasswordForm = this._formBuilder.group({
            username: [username, Validators.required],
            resetToken: [resettoken, Validators.required],
            password: ['', Validators.required],
            passwordConfirm: ['', Validators.required]
        },
            {
                validators: FuseValidators.mustMatch('password', 'passwordConfirm')
            }
        );
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Reset password
     */
    resetPassword(): void {
        // Return if the form is invalid
        if (this.resetPasswordForm.invalid) {
            return;
        }

        // Disable the form
        this.resetPasswordForm.disable();

        // Hide the alert
        this.showAlert = false;

        // Send the request to the server
        this._authService.resetPassword(this.resetPasswordForm.value)
            .pipe(
                finalize(() => {

                    // Re-enable the form
                    this.resetPasswordForm.enable();

                    // Reset the form
                    //this.resetPasswordNgForm.resetForm();
                })
            )
            .subscribe(
                (result) => {

                    if (result.status === StatusEnum.Success) {

                        swal.fire(
                            'Password Reset',
                            'Your reset was successful, please login with new password.',
                            'success'
                        ).then(() => {
                            this._router.navigate(['/sign-in']);
                        }
                        );
                    }
                    else {

                        var errorMessage = '';

                        if (result.data && result.data.length) {
                            errorMessage = "There was an error creating your account.\nPlease fix the following and recreate your account: \n";
                            for (var i = 0; i < result.data.length; i++) {
                                errorMessage += '- ' + result.data[i] + '\n';
                            }
                        }
                        else
                            errorMessage = result.data.toString();

                        this.alert = {
                            type: 'error',
                            message: errorMessage,
                        };

                        // Show the alert
                        this.showAlert = true;
                    }

                },
                (response) => {

                    // Set the alert
                    this.alert = {
                        type: 'error',
                        message: 'Something went wrong, please try again.'
                    };

                    // Show the alert
                    this.showAlert = true;
                }
            );
    }
}
