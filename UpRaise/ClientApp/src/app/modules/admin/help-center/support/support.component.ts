import { ChangeDetectorRef, Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { fuseAnimations } from '@fuse/animations';
import { HelpCenterService } from 'app/modules/admin/help-center/help-center.service';
import { IUser } from 'app/models/user';
import { UserService } from 'app/core/user/user.service';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { StatusEnum } from '../../../../models';

@Component({
    selector     : 'help-center-support',
    templateUrl  : './support.component.html',
    encapsulation: ViewEncapsulation.None,
    animations   : fuseAnimations
})
export class HelpCenterSupportComponent implements OnInit
{
    @ViewChild('supportNgForm') supportNgForm: NgForm;
    user: IUser;
    alert: any;
    supportForm: FormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    /**
     * Constructor
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _formBuilder: FormBuilder,
        private _helpCenterService: HelpCenterService,
        private _userService: UserService,
    )
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        this._userService.user$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((user: IUser) => {
                this.user = user;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });


        // Create the support form
        this.supportForm = this._formBuilder.group({
            subject: ['', Validators.required],
            message: ['', Validators.required]
        });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Clear the form
     */
    clearForm(): void
    {
        // Reset the form
        this.supportNgForm.resetForm();
    }

    /**
     * Send the form
     */
    sendForm(): void
    {
        // Return if the form is invalid
        if (this.supportForm.invalid) {
            return;
        }

        this.alert = null;
        // Disable the form
        this.supportForm.disable();

        // Hide the alert
        //this.showAlert = false;

        this._helpCenterService.support(this.supportForm.value)
            .subscribe((response: any) => {
                if (response.status === StatusEnum.Success) {

                    this.alert = {
                        type: 'success',
                        message: 'Your request has been delivered! A member of our support staff will respond as soon as possible.'
                    };
                }
                else {

                    this.alert = {
                        type: 'error',
                        message: 'Failed sending in support message.  Please try again in a bit.'
                    };
                }

                // Clear the form
                this.clearForm();

                this.supportForm.enable();
            });



    }
}
