import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSelectChange } from '@angular/material/select';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { BehaviorSubject, combineLatest, fromEvent, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { IPublicCampaignDetail } from '../../home.types';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import swal, { SweetAlertOptions } from 'sweetalert2';
import { ContributionService } from '../contribution.service';
import { ContributionTypes } from '../contribution.enums';
import { ResultDTO, StatusEnum } from '../../../../../models';
import { ICryptoCreateResponseDTO } from '../contribution.types';

@Component({
    selector: 'crypto',
    templateUrl: './crypto.component.html',
    styleUrls: ['./crypto.component.styles.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CryptoComponent implements OnInit, OnDestroy {

    @Input() public campaign: IPublicCampaignDetail;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    public formGroup: FormGroup;

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
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------
    async onSubmitCrypto() {
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

        const router = this._router;
        const campaign = this.campaign;
        const formValue = this.formGroup.value;
        const contributionService = this._contributionService;

        this._contributionService.cryptocreate(campaign.type, this.campaign.id, this.formGroup.value.amount, this.formGroup.value.comment)
            .subscribe((resultDTO: ResultDTO) => {

                if (resultDTO.status == StatusEnum.Success) {
                    const cryptoResponse: ICryptoCreateResponseDTO = resultDTO.data;
                    window.location.href = cryptoResponse.url;
                }
            },
                error => {
                    console.log(error);
                    this.formGroup.enable();
                },
                () => {
                    this.formGroup.enable();
                });



        /*
        this._contributionService
            .contribution(ContributionTypes.Crypto, campaign.type, campaign.id, formValue.amount, formValue.comment)
            .subscribe(
                (response) => {

                    swal.fire('Contribution', "Thank you for your contribution.", 'success')
                        .then((result) => {
                            router.navigate(['/home/detail', campaign.type, campaign.id]);
                        });
                },
                (err) => {
                    this.formGroup.enable();
                },
                () => {
                    this.formGroup.enable();
                });
        */

    }
}
