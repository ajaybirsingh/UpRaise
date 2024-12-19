import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import swal, { SweetAlertOptions } from 'sweetalert2';
import { StatusEnum } from 'app/models';
import { Component, OnInit, ViewEncapsulation, ViewChild } from '@angular/core';
import { FuseUtilsService } from '@fuse/services/utils/utils.service';
import { FormBuilder, FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { CampaignService } from '../../../../core/campaigns/campaigns.service';
import { newArray } from '@angular/compiler/src/util';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { SaveCampaignComponent } from './save-campaign.component';
import { ActivatedRoute, ActivatedRouteSnapshot, Route, Router, RouterStateSnapshot } from '@angular/router';
import { v4 as uuidv4 } from 'uuid';
import { CampaignTypes, Countries } from '../../../../shared/enums';
import { IEditCampaign, WizardEventTypes } from './new-campaign.types';
import { PeopleModule } from './steps/people/people.module';
import { OrganizationModule } from './steps/organization/organization.module';
import { FuseNavigationService, FuseVerticalNavigationComponent } from '@fuse/components/navigation';
import { NewCampaignService } from './new-campaign.service';

//https://www.figma.com/file/qQV3tOzlaf9Y6ZDoujZy6l/Upraise-Experience-Architecture?node-id=45%3A0
@Component({
    selector: 'new-campaign',
    templateUrl: './new-campaign.component.html',
    encapsulation: ViewEncapsulation.None,
    styleUrls: ['./new-campaign.styles.css']
})
export class NewCampaignComponent implements OnInit  {
    public campaignTypes = CampaignTypes;

    private transactionId: string = uuidv4();
    private _campaignId?: number = null;
    public IsEditing: boolean = false;
    //public randomId = '';
    public currentStep = 0;

    constructor(
        private _router: Router,
        private _formBuilder: FormBuilder,
        private _fuseUtilsService: FuseUtilsService,
        private _campaignService: CampaignService,
        private _matDialog: MatDialog,
        private _activatedRoute: ActivatedRoute,
        private _fuseNavigationService: FuseNavigationService
    ) {

    }


    private isStepValid = (index: number): boolean => {
        return this.getGroupAt(index).valid || this.currentGroup.untouched;
    };

    private shouldValidate = (index: number): boolean => {
        return this.getGroupAt(index).touched && this.currentStep >= index;
    };

    toggleNavigation(name: string): void {
        // Get the navigation
        const navigation = this._fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(name);

        if (navigation) {
            // Toggle the opened status
            navigation.toggle();
        }
    }

    public formCampaignType = new FormGroup({
        step1: new FormGroup({
            campaignTypeId: new FormControl(null, Validators.required),
        })
    });

    public formOrganization = new FormGroup({
        step2: new FormGroup({
            categoryId: new FormControl(1),
            name: new FormControl('', Validators.required),
            description: new FormControl(''),//, Validators.required

            geoLocationCountryId: new FormControl(),
            geoLocationAddress: new FormControl('', Validators.required),
            geoLocationLatitude: new FormControl(Validators.required),
            geoLocationLongitude: new FormControl(Validators.required),
        }),
        step3: new FormGroup({
            beneficiaryOrganization: new FormControl(''), //, Validators.required
            location: new FormControl(''), //, [Validators.required, Validators.email]
            contactName: new FormControl(''), //, Validators.required
            contactEmail: new FormControl(''), //, Validators.required
            contactPhone: new FormControl(''), //, Validators.required
        }),
        step4: new FormGroup({
            fundraisingGoals: new FormControl('', Validators.required),
            currencyId: new FormControl(1),
            startDate: new FormControl(''),
            endDate: new FormControl(''),
            campaignConditions: new FormArray([]),
        }),
        step5: new FormGroup({
            distributionTerms: new FormArray([]),
        }),
        step6: new FormGroup({
            headerPhoto: new FormControl(''),
            photos: new FormArray([]),
            videos: new FormArray([]),
        }),
        step7: new FormGroup({
            acceptTermsAndConditions: new FormControl(false, checkboxTrueValidator),
        }),
        step8: new FormGroup({
            beneficiaryMessage: new FormControl(''),
        }),
    });


    public formPeople = new FormGroup({
        step2: new FormGroup({
            name: new FormControl('', Validators.required),
            description: new FormControl(''),
            fundraisingGoals: new FormControl('', Validators.required),
            currencyId: new FormControl(1),
            geoLocationCountryId: new FormControl(),
            geoLocationAddress: new FormControl('', Validators.required),
            geoLocationLatitude: new FormControl(Validators.required),
            geoLocationLongitude: new FormControl(Validators.required),
        }),
        step3: new FormGroup({
            distributionTerms: new FormArray([]),
        }),
        step4: new FormGroup({
            beneficiaryName: new FormControl('', Validators.required),
            beneficiaryEmail: new FormControl('', Validators.required),
            beneficiaryMessage: new FormControl(''),
        }),
        step5: new FormGroup({
            headerPhoto: new FormControl(''),
            photos: new FormArray([]),
            videos: new FormArray([]),
        }),
        step6: new FormGroup({
            acceptTermsAndConditions: new FormControl(false, checkboxTrueValidator),
        })
    });


    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    ngOnInit(): void {

        this.toggleNavigation("mainNavigation");

        const editCampaign = this._activatedRoute.snapshot.data['editCampaign'];

        if (editCampaign) {
            this.IsEditing = true;
            this.populateExistingCampaign(editCampaign);
        }


        //this.news = this.route.snapshot.data['news'];


        //this.randomId = this._fuseUtilsService.randomId();

        //this.populatePeopleTestData();
        //this.populateOrganizationTestData();
    }



    private populateExistingCampaign(campaign: IEditCampaign) {

        this.currentStep = 1; //this is step 2

        this.transactionId = campaign.transactionId;
        this._campaignId = campaign.id;

        this.formCampaignType.controls["step1"].reset({
            campaignTypeId: campaign.typeId,
        });


        switch (campaign.typeId) {

            case CampaignTypes.Organization:
                {
                    this.formOrganization.controls["step2"].reset({
                        categoryId: campaign.organization.categoryId,
                        name: campaign.name,
                        description: campaign.description,

                        geoLocationCountryId: campaign.geoLocationCountryId,
                        geoLocationAddress: campaign.geoLocationAddress,
                    });

                    this.formOrganization.controls["step3"].reset({
                        beneficiaryOrganization: campaign.organization.beneficiaryOrganization,
                        location: campaign.organization.location,
                        contactName: campaign.organization.contactName,
                        contactEmail: campaign.organization.contactEmail,
                        contactPhone: campaign.organization.contactPhone,
                    });

                    this.formOrganization.controls["step4"].reset({
                        fundraisingGoals: campaign.fundraisingGoals,
                        currencyId: campaign.currencyId,
                        startDate: campaign.startDate,
                        endDate: campaign.endDate,
                        //campaignConditions: campaign.organization.campaignConditions,
                    });

                    var campaignConditionsTermArray = this.formOrganization.controls["step4"].get('campaignConditions') as FormArray;
                    if (campaign.organization.campaignConditions) {
                        for (let item of campaign.organization.campaignConditions) {
                            var term = this._formBuilder.group({
                                label: item,
                                removable: true,
                            });
                            campaignConditionsTermArray.push(term);
                        };
                    }


                    var distributionTermArray = this.formOrganization.controls["step5"].get('distributionTerms') as FormArray;
                    if (campaign.distributionTerms) {
                        for (let item of campaign.distributionTerms) {
                            var term = this._formBuilder.group({
                                label: item,
                                removable: true,
                            });
                            distributionTermArray.push(term);
                        };
                    }


                    this.formOrganization.controls["step6"].reset({
                        headerPhoto: campaign.headerPhoto,
                        //photos: campaign.organization.photos,
                        //videos: campaign.organization.videos,
                    });

                    var photosArray = this.formOrganization.controls["step6"].get('photos') as FormArray;
                    if (campaign.photos) {
                        for (let item of campaign.photos) {
                            var photo = this._formBuilder.group(item);
                            photosArray.push(photo);
                        };
                    }

                    var videosArray = this.formOrganization.controls["step6"].get('videos') as FormArray;
                    if (campaign.videos) {
                        for (let item of campaign.videos) {
                            var video = this._formBuilder.group(item);
                            videosArray.push(video);
                        };
                    }

                    this.formOrganization.controls["step7"].reset({
                        acceptTermsAndConditions: campaign.acceptTermsAndConditions,
                    });

                    this.formOrganization.controls["step8"].reset({
                        beneficiaryMessage: campaign.organization.beneficiaryMessage,
                    });

                }
                break;

            case CampaignTypes.People:
                {

                    this.formPeople.controls["step2"].reset({
                        name: campaign.name,
                        description: campaign.description,
                        fundraisingGoals: campaign.fundraisingGoals,
                        currencyId: campaign.currencyId,
                        geoLocationCountryId: campaign.geoLocationCountryId,
                        geoLocationAddress: campaign.geoLocationAddress,
                    });

                    var distributionTermArray = this.formPeople.controls["step3"].get('distributionTerms') as FormArray;
                    if (campaign.distributionTerms) {
                        for (let item of campaign.distributionTerms) {
                            var term = this._formBuilder.group({
                                label: item,
                                removable: true,
                            });
                            distributionTermArray.push(term);
                        };
                    }

                    this.formPeople.controls["step4"].reset({
                        beneficiaryName: campaign.people.beneficiaryName,
                        beneficiaryEmail: campaign.people.beneficiaryEmail,
                        beneficiaryMessage: campaign.people.beneficiaryMessage,
                    });

                    this.formPeople.controls["step5"].reset({
                        headerPhoto: campaign.headerPhoto,
                        //photos: campaign.photos,
                        //videos: campaign.videos,
                    });


                    var photosArray = this.formPeople.controls["step5"].get('photos') as FormArray;
                    if (campaign.photos) {
                        for (let item of campaign.photos) {
                            var photo = this._formBuilder.group(item);
                            photosArray.push(photo);
                        };
                    }

                    var videosArray = this.formPeople.controls["step5"].get('videos') as FormArray;
                    if (campaign.videos) {
                        for (let item of campaign.videos) {
                            var video = this._formBuilder.group(item);
                            videosArray.push(video);
                        };
                    }

                    this.formPeople.controls["step6"].reset({
                        acceptTermsAndConditions: campaign.acceptTermsAndConditions,
                    });

                }
                break;
        }
    }

    private populatePeopleTestData() {
        this.currentStep = 1;

        this.formCampaignType.controls["step1"].reset({
            campaignTypeId: CampaignTypes.People,
        });

        this.formPeople.controls["step2"].reset({
            name: 'test name',
            description: 'Test description',
            fundraisingGoals: 16000,
            geoLocationCountryId: Countries.Canada,
            geoLocationAddress: '',
        });

        this.formPeople.controls["step3"].reset({
            distributionTerms: this._formBuilder.array([])
        });

        this.formPeople.controls["step4"].reset({
            beneficiaryName: 'Sunny',
            beneficiaryEmail: 'sbhuller@outlook.com',
        });

        this.formPeople.controls["step5"].reset({
            photos: this._formBuilder.array([]),
            videos: this._formBuilder.array([])
        });

        this.formPeople.controls["step6"].reset({
            acceptTermsAndConditions: true,
        });

    }


    private populateOrganizationTestData() {
        this.currentStep = 5;

        this.formCampaignType.controls["step1"].reset({
            campaignTypeId: CampaignTypes.Organization,
        });

        this.formOrganization.controls["step2"].reset({
            categoryId: 1,
            name: 'test name',
            description: 'Test description',

            geoLocationCountryId: Countries.Canada,
            geoLocationAddress: '',
        });

        this.formOrganization.controls["step3"].reset({
            location: 'testloc',
            contactName: 'cphone',
            contactEmail: 'c@c.out.com',
            contactPhone: 'cphone'
        });

        this.formOrganization.controls["step4"].reset({
            fundraisingGoals: 4.35,
            startDate: new Date(2011, 4, 6),
            endDate: new Date(2011, 5, 8),
        });

        var campaignConditions = this.formOrganization.controls["step4"].value.campaignConditions as FormArray;

        var campaignConditionOne = this._formBuilder.group({
            label: 'test one',
            removable: true,
        });
        campaignConditions.push(campaignConditionOne);

        var campaignConditionTwo = this._formBuilder.group({
            label: 'test two',
            removable: true,
        });
        campaignConditions.push(campaignConditionTwo);
        //var campaignConditions = <FormArray>this.formOrganization.controls["step3"].value.campaignConditions;
        //campaignConditions.push(new FormControl('test'));
        //campaignConditions.push(new FormControl('another sample'));

        this.formOrganization.controls["step5"].reset({
            distributionTerms: this._formBuilder.array([])
        });

        //this.formOrganization.controls["step4"].reset({
        //distributionTerms: this._formBuilder.array([])
        //});

        //var distributionTerm = <FormArray>this.formOrganization.controls["step3"].value.distributionTerm;
        //distributionTerm.push(this._formBuilder.group({ term: 'dist1' }));
        //distributionTerm.push(this._formBuilder.group({ term: 'dist2' }));

        this.formOrganization.controls["step7"].reset({
            acceptTermsAndConditions: true,
        });
    }

    public get currentGroup(): FormGroup {
        return this.getGroupAt(this.currentStep);
    }

    public getCampaignType(): CampaignTypes {
        if (this.formCampaignType) {
            const campaignType = this.formCampaignType.controls['step1'].value.campaignTypeId;
            return campaignType;
        }
        return null;
    }

    public next(): void {
        switch (this.getCampaignType()) {
            case CampaignTypes.Organization:
                {
                    if (this.currentGroup.valid && (this.currentStep + 1) < OrganizationModule.maxStep) {
                        this.currentStep += 1;
                        return;
                    }

                    this.currentGroup.markAllAsTouched();

                    //this.stepperOrganization.validateSteps();
                }
                break;
            case CampaignTypes.People:
                {
                    if (this.currentGroup.valid && (this.currentStep + 1) < PeopleModule.maxStep) {
                        this.currentStep += 1;
                        return;
                    }

                    this.currentGroup.markAllAsTouched();

                    //this.stepperPeople.validateSteps();
                }

                break;
        }


    }

    public prev(): void {
        if (this.currentStep == 0) {
            this.formCampaignType.reset();
            this.formOrganization.reset();
            this.formPeople.reset();
        }
        else {
            this.currentStep -= 1;
        }
    }

    private getGroupAt(index: number): FormGroup {


        if (index == 0) {
            groups = Object.keys(this.formCampaignType.controls).map(groupName => this.formCampaignType.get(groupName)) as FormGroup[];
            return groups[0];
        }
        else {
            var groups: FormGroup[];
            switch (this.getCampaignType()) {
                case CampaignTypes.Organization:
                    groups = Object.keys(this.formOrganization.controls).map(groupName => this.formOrganization.get(groupName)) as FormGroup[];
                    break;
                case CampaignTypes.People:
                    groups = Object.keys(this.formPeople.controls).map(groupName => this.formPeople.get(groupName)) as FormGroup[];
                    break;
            }
            return groups[index - 1];
        }


    }

    public getWizardEvent(eventType: WizardEventTypes) {
        switch (eventType) {

            case WizardEventTypes.Next:
                this.next();
                break;

            case WizardEventTypes.Previous:
                this.prev();
                break;

            case WizardEventTypes.GoLive:

                switch (this.getCampaignType()) {
                    case CampaignTypes.Organization:
                        this.submitOrganizationCampaign();
                        break;

                    case CampaignTypes.People:
                        this.submitPeopleCampaign();
                        break;
                }

                break;

            case WizardEventTypes.SaveDraft:
                break;
        }
    }

    public stepProgess(): number {

        var progress = 0;

        switch (this.getCampaignType()) {
            case CampaignTypes.Organization:
                progress = ((this.currentStep + 1) / (OrganizationModule.maxStep)) * 100.0;
                break;
            case CampaignTypes.People:
                progress = ((this.currentStep + 1) / (PeopleModule.maxStep)) * 100.0;
                break;
        }

        return progress;
    }

    public campaignTypeSubTitle(): string {

        switch (this.getCampaignType()) {
            case CampaignTypes.Organization:
                return 'for Organization';

            case CampaignTypes.People:
                return 'for People';
        }

        return '';
    }

    public getBeneficiaryName(): string {

        const beneficiaryName = this.formPeople.controls['step4'].value.beneficiaryName;
        return beneficiaryName;
    }


    private getFileObject(item: any): any {
        if (item) {
            if (item.id) {
                return { Id: item.id };
            }
            else {
                if (item.fileInfo) {
                    return {
                        UID: item.fileInfo.uid,
                        FileName: item.fileInfo.name,
                        FileSize: item.fileInfo.size
                    }
                }
            }

        }

        return null;
    }

    public submitPeopleCampaign(): void {
        if (!this.currentGroup.valid) {
            this.currentGroup.markAllAsTouched();
            //this.stepperOrganization.validateSteps();
        }
        if (this.formPeople.valid) {
            this.formPeople.disable();

            const savePeopleCampaign = {
                ...this.formPeople.controls["step2"].value,
                ...this.formPeople.controls["step3"].value,
                ...this.formPeople.controls["step5"].value,
                ...this.formPeople.controls["step6"].value,
                //...this.formPeople.controls["step8"].value,
            };


            if (this._campaignId)
                savePeopleCampaign.Id = this._campaignId;

            savePeopleCampaign.typeId = CampaignTypes.People;
            savePeopleCampaign.transactionId = this.transactionId;

            //
            //people specific beneficiary data
            //
            savePeopleCampaign.peopleCampaignDTO = {
                ...this.formPeople.controls["step4"].value
            };


            var distributionTerms = savePeopleCampaign.distributionTerms.map((item) => {
                return item.label;
            });
            savePeopleCampaign.distributionTerms = distributionTerms;

            var headerPhoto = this.formPeople.controls["step5"].value.headerPhoto;
            if (headerPhoto) {
                savePeopleCampaign.headerPhoto = this.getFileObject(headerPhoto);

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                if (!headerPhoto.fileInfo)
                    headerPhoto = null;
            }

            var photos = this.formPeople.controls["step5"].value.photos
            if (photos) {
                savePeopleCampaign.photos = photos.map((item) => {
                    return this.getFileObject(item);
                });

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                photos = photos.filter(function (obj) {
                    return obj.fileInfo;
                });

            }

            var videos = this.formPeople.controls["step5"].value.videos
            if (videos) {
                savePeopleCampaign.videos = videos.map((item) => {
                    return this.getFileObject(item);
                });

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                videos = videos.filter(function (obj) {
                    return obj.fileInfo;
                });

            }

            //console.log('Save people campaign data', savePeopleCampaign);

            const dialogConfig = new MatDialogConfig();

            dialogConfig.disableClose = true;
            dialogConfig.autoFocus = true;

            dialogConfig.data = {
                campaignType: CampaignTypes.People,
                data: savePeopleCampaign,
                headerPhotoFile: headerPhoto,
                photoFiles: photos,
                videoFiles: videos,
                transactionId: this.transactionId
            };

            const dialogRef = this._matDialog.open(SaveCampaignComponent, dialogConfig);
            dialogRef.afterClosed().subscribe(
                data => {
                    //console.log("Dialog output:", data);

                    this.formPeople.enable();

                    if (data.status) {
                        this._router.navigate(['/home/detail', CampaignTypes.People, data.id]);
                    }

                }
            );

        }
    }

    public submitOrganizationCampaign(): void {

        if (!this.currentGroup.valid) {
            this.currentGroup.markAllAsTouched();
            //this.stepperOrganization.validateSteps();
        }
        if (this.formOrganization.valid) {
            this.formOrganization.disable();

            const saveOrganizationCampaign = {
                ...this.formOrganization.controls["step2"].value,
                //...this.formOrganization.controls["step3"].value,
                ...this.formOrganization.controls["step4"].value,
                ...this.formOrganization.controls["step5"].value,
                ...this.formOrganization.controls["step6"].value,
                ...this.formOrganization.controls["step7"].value,
                ...this.formOrganization.controls["step8"].value
                
            };

            if (this._campaignId)
                saveOrganizationCampaign.Id = this._campaignId;

            saveOrganizationCampaign.typeId = CampaignTypes.Organization;
            saveOrganizationCampaign.transactionId = this.transactionId;

            //
            //people specific beneficiary data
            //
            saveOrganizationCampaign.organizationCampaignDTO = {
                categoryID: this.formOrganization.controls["step2"].value.categoryId,
                ...this.formOrganization.controls["step3"].value,
                campaignConditions: this.formOrganization.controls["step4"].value.campaignConditions,
                ...this.formOrganization.controls["step8"].value,
            };

            var campaignConditions = saveOrganizationCampaign.organizationCampaignDTO.campaignConditions.map((item) => {
                return item.label;
            });
            saveOrganizationCampaign.organizationCampaignDTO.campaignConditions = campaignConditions;


            var distributionTerms = saveOrganizationCampaign.distributionTerms.map((item) => {
                return item.label;
            });
            saveOrganizationCampaign.distributionTerms = distributionTerms;


            var headerPhoto = this.formOrganization.controls["step6"].value.headerPhoto;
            if (headerPhoto) {
                saveOrganizationCampaign.headerPhoto = this.getFileObject(headerPhoto);

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                if (!headerPhoto.fileInfo)
                    headerPhoto = null;
            }


            var photos = this.formOrganization.controls["step6"].value.photos
            if (photos) {
                saveOrganizationCampaign.photos = photos.map((item) => {
                    return this.getFileObject(item);
                });

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                photos = photos.filter(function (obj) {
                    return obj.fileInfo;
                });
            }

            var videos = this.formOrganization.controls["step6"].value.videos
            if (videos) {
                saveOrganizationCampaign.videos = videos.map((item) => {
                    return this.getFileObject(item);
                });

                //
                //if this isn't a new file with binary data then remove it ( already uploaded )
                //
                videos = videos.filter(function (obj) {
                    return obj.fileInfo;
                });
            }

            //console.log('Save organization campaign data', saveOrganizationCampaign);

            const dialogConfig = new MatDialogConfig();

            dialogConfig.disableClose = true;
            dialogConfig.autoFocus = true;

            dialogConfig.data = {
                campaignType: CampaignTypes.Organization,
                data: saveOrganizationCampaign,
                headerPhotoFile: headerPhoto,
                photoFiles: photos,
                videoFiles: videos,
                transactionId: this.transactionId
            };

            const dialogRef = this._matDialog.open(SaveCampaignComponent, dialogConfig);
            dialogRef.afterClosed().subscribe(
                data => {
                    //console.log("Dialog output:", data);

                    this.formOrganization.enable();

                    if (data) {
                        this._router.navigate(['/home/detail', CampaignTypes.Organization, data.id]);
                    }

                }
            );

        }

    }



}



export const checkboxTrueValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {

    if (!control.parent || !control) {
        return null;
    }

    const cb = control.parent.get('acceptTermsAndConditions');

    if (cb.value === true) {
        return null;
    }

    return { 'cbNotTrue': true };
}
