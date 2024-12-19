import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import swal, { SweetAlertOptions } from 'sweetalert2';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSelectChange } from '@angular/material/select';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { BehaviorSubject, combineLatest, fromEvent, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { ContributionService } from '../contribution.service';
import { ResultDTO, StatusEnum } from '../../../../../models';
import { IStripeCreateResponseDTO } from '../contribution.types';
import { AfterViewInit } from '@angular/core';
import { FormControl, FormGroup, NgForm, Validators } from '@angular/forms';
import { IPublicCampaignDetail } from '../../home.types';
import { ContributionTypes } from '../contribution.enums';

//
//https://stripe.com/docs/testing
//
declare var Stripe: any;

@Component({
    selector: 'stripe',
    templateUrl: './stripe.component.html',
    styleUrls: ['./stripe.component.styles.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StripeComponent implements OnInit, OnDestroy, AfterViewInit {

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    @ViewChild('cardInfo') cardInfo: ElementRef; //import viewChild ElementRef
    cardHandler = this.onChange.bind(this);
    error: string;
    stripe: any;
    card: any;
    //@ViewChild('query', {static: true}) query: ElementRef;
    //@Input() amount;
    //@Input() description;

    //handler: StripeCheckoutHandler;
    public formGroup: FormGroup;

    confirmation: any;
    loading = false;

    @Input() public campaign: IPublicCampaignDetail;

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _router: Router,
        private _contributionService: ContributionService
    ) {

        this.formGroup = new FormGroup({
            'comment': new FormControl(''),
            'amount': new FormControl(0, [Validators.max(10000), Validators.min(0)]),
        });

    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    ngAfterViewInit(): void {
        // Your Stripe public key
        this.stripe = Stripe('pk_test_51Ja4hEDwqq579CoGmOCmCIafY0EsE1TvOsPjTR6YqMNBRjYapxYrWk865WIk40PineHbfSFYvooIq8zHaUSouczO00p1o7Yk9T');

        const elements = this.stripe.elements();

        this.card = elements.create('card');

        this.card.mount(this.cardInfo.nativeElement);
        this.card.addEventListener('change', this.cardHandler);
    }

    onChange(error) {
        if (error) {
            this.error = error.message;
        } else {
            this.error = null;
        }
        this._changeDetectorRef.detectChanges();
    }


    /**
     * On init
     */
    ngOnInit(): void {

    }


    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();

        this.card.removeEventListener('change', this.cardHandler);
        this.card.destroy();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    // Close on navigate
    //@HostListener('window:popstate')
    //onPopstate() {
    //this.handler.close();
    //}

    async onSubmitStripe() { // import NgForm

        for (var i in this.formGroup.controls) {
            this.formGroup.controls[i].markAsTouched();
        }

        if (this.formGroup.value.amount == 0) {
            swal.fire('Amount', "Contributions must be greater than $0.00", 'error');
            return;
        }

        if (this.formGroup.invalid)
            return;

        this.formGroup.disable();

        const { token, error } = await this.stripe.createToken(this.card);

        if (error) {
            console.log('Something is wrong:', error);
            this.formGroup.enable();
        } else {
            console.log('Success!', token); //response 

            const router = this._router;
            const campaign = this.campaign;
            const formValue = this.formGroup.value;
            const contributionService = this._contributionService;

            this._contributionService.stripecreate(this.campaign.id, this.formGroup.value.amount, token)
                .subscribe((resultDTO: ResultDTO) => {

                    if (resultDTO.status == StatusEnum.Success) {
                        const stripeResponse: IStripeCreateResponseDTO = resultDTO.data;

                        //confirmCardPayment
                        this.stripe.confirmCardPayment(stripeResponse.clientSecret, {
                            payment_method: {
                                card: this.card
                            }
                        })
                            .then(function (result) {
                                // Handle result.error or result.paymentIntent

                                if (result.error) {
                                    console.error(result.error);
                                }
                                else {
                                    if (result.paymentIntent.status == 'succeeded') {

                                        contributionService
                                            .contribution(ContributionTypes.CreditCard, campaign.type, campaign.id, formValue.amount, formValue.comment)
                                            .subscribe(
                                                (response) => {
                                                    // do something when data is received
                                                    //console.log(response);
                                                },
                                                (err) => {
                                                    // do something on error
                                                    //console.error(err)
                                                },
                                                () => {
                                                    // do something when operation successfully complete
                                                    //console.log('success!');

                                                    swal.fire('Contribution', "Thank you for your contribution.", 'success')
                                                        .then((result) => {
                                                            router.navigate(['/home/detail', campaign.type, campaign.id]);
                                                        });

                                                });



                                    }
                                }

                                this.formGroup.enable();
                            });
                    }
                },
                    error => {
                        console.log(error);
                        this.formGroup.enable();
                    },
                    () => {
                        this.formGroup.enable();
                    });
        }

        //e.preventDefault();
    }

}
