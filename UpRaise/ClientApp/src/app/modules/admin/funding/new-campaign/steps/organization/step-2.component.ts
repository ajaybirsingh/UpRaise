import { animate, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { EnumsService } from 'app/core/services/enums.service';
import { ResultDTO, EnumDTO, StatusEnum } from 'app/models/index';
import { WizardEventTypes } from '../../new-campaign.types';
import 'quill-emoji/dist/quill-emoji';
import Quill from 'quill'
import { MatDialog } from '@angular/material/dialog';
import { GeocodeDialog } from '../../../../../../components/geocode/geocode.dialog';

@Component({
    selector: 'org-step-2',
    templateUrl: './step-2.component.html',
    styleUrls: ['./step-2.component.css'],
    animations: [
        trigger("toggleAnimation", [
            transition(":enter", [
                style({ opacity: 0, transform: "scale(0.95)" }),
                animate("100ms ease-out", style({ opacity: 1, transform: "scale(1)" }))
            ]),
            transition(":leave", [
                animate("75ms", style({ opacity: 0, transform: "scale(0.95)" }))
            ])
        ])
    ]
})
export class Step2Component implements AfterViewInit {
    @Input() public stepData: FormGroup;
    @Input() public isEditing: boolean;

    @Output() WizardEvent = new EventEmitter<WizardEventTypes>();

    @ViewChild('geoLocation') private _geoLocation: ElementRef;

    public organizationCampaignCategories: EnumDTO[];

    public charactersCountCampaign = '';
    public charactersCountDescription = '';

    constructor(
        private _enumsService: EnumsService,
        private dialog: MatDialog) {
        this.updateCharactersCountCampaign(0);
        this.updateCharactersCountDescription(0);
    }

    public async ngAfterViewInit(): Promise<void> {
        this.organizationCampaignCategories = await this._enumsService.GetOrganizationCampaignCategories();
        this.quillInit();
    }

    private editor: any;

    quillInit() {

        this.editor = new Quill('#editor-container', {
            modules: {
                //blotFormatter: {},
                'emoji-shortname': true,
                'emoji-textarea': false,
                'emoji-toolbar': true,
                toolbar: [
                    ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
                    ['blockquote', 'code-block'],

                    [{ 'header': 1 }, { 'header': 2 }],               // custom button values
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
                    [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
                    [{ 'direction': 'rtl' }],                         // text direction

                    [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
                    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],

                    [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
                    [{ 'font': [] }],
                    [{ 'align': [] }],

                    ['clean'],                                         // remove formatting button

                    //['link', 'image', 'video'],                         // link and image, video
                    ['emoji']
                ]
            },
            scrollingContainer: '#scrolling-container',
            placeholder: 'Compose an epic...',
            theme: 'snow'
        });


        this.editor.root.innerHTML = this.stepData.get('description').value;

        //let _this = this;

        /*
        // While we type, copy the text to our hidden form field so it can be saved.
        editor.on('text-change', function (delta) {
            _this.stepData.get('description').setValue(this.editor.root.innerHTML);
        });
        */

        // Capture focus on and off events
        //editor.on('selection-change', function (range, oldRange, source) {
        //if (range === null && oldRange !== null) {
        //_this.onFocusOut();
        //} else if (range !== null && oldRange === null)
        //_this.onFocus();
        //});
    }

    private updateStringLength(length: number, maxLength: number): string {
        return `${length}/${maxLength}`;
    }

    public updateCharactersCountCampaign(length: number) {
        this.charactersCountCampaign = this.updateStringLength(length, 100);
    }

    public updateCharactersCountDescription(length: number) {
        this.charactersCountDescription = this.updateStringLength(length, 1000);
    }


    public prev() {
        this.stepData.get('description').setValue(this.editor.root.innerHTML);
        this.WizardEvent.emit(WizardEventTypes.Previous);
    }


    public next() {
        this.stepData.get('description').setValue(this.editor.root.innerHTML);
        this.WizardEvent.emit(WizardEventTypes.Next);
    }

    addBindingCreated(quill) {
        quill.keyboard.addBinding({
            key: 'b'
        }, (range, context) => {
            // tslint:disable-next-line:no-console
            console.log('KEYBINDING B', range, context)
        })

        quill.keyboard.addBinding({
            key: 'B',
            shiftKey: true
        }, (range, context) => {
            // tslint:disable-next-line:no-console
            console.log('KEYBINDING SHIFT + B', range, context)
        })
    }


    public clickCategory(item: EnumDTO) {
        this.stepData.get('categoryId').setValue(item.value);
    }

    public getCategoryDescription(categoryId?: number) {
        if (this.organizationCampaignCategories) {
            if (categoryId) {
                var category = this.organizationCampaignCategories.find(x => x.value == categoryId);
                if (category)
                    return category.description;
            }
        }

        return '';
    }

    public clickLocation() {

        const geoLocationAddress = this.stepData.get('geoLocationAddress').value;
        const geoLocationLatitude = this.stepData.get('geoLocationLatitude').value;
        const geoLocationLongitude = this.stepData.get('geoLocationLongitude').value;

        const dialogRef = this.dialog.open(GeocodeDialog, {
            //panelClass: 'mail-ngrx-compose-dialog'
            disableClose: true,
            data: { formattedAddress: geoLocationAddress, latitude: geoLocationLatitude, longitude: geoLocationLongitude }
        });


        dialogRef.afterClosed().subscribe(result => {

            if (result) {
                
                this.stepData.get('geoLocationAddress').setValue(result.formattedAddress);
                this.stepData.get('geoLocationLatitude').setValue(result.latitude);
                this.stepData.get('geoLocationLongitude').setValue(result.longitude);

            }
        });
    }

}
