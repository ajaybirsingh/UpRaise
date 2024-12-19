import { EventEmitter, AfterViewInit, Component, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { WizardEventTypes } from '../../new-campaign.types';
import 'quill-emoji/dist/quill-emoji';
import Quill from 'quill'
import { EnumDTO } from '../../../../../../models';
import { EnumsService } from '../../../../../../core/services/enums.service';
import { animate, style, transition, trigger } from '@angular/animations';
import { Countries } from '../../../../../../shared/enums';
import { NewCampaignService } from '../../new-campaign.service';
import { GeocodeDialog } from 'app/components/geocode/geocode.dialog';
import { MatDialog } from '@angular/material/dialog';

//import BlotFormatter from "quill-blot-formatter";
//Quill.register("modules/blotFormatter", BlotFormatter);

//https://github.com/KillerCodeMonkey/ngx-quill-example/blob/e7b0c27dfa7f50f0dda89712575a14005135f7d5/src/app/formula/formula.component.ts
//https://blog.almightytricks.com/2020/10/21/ngx-quill-image-resize/
@Component({
    selector: 'people-step-2',
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

    public countries: EnumDTO[];

    constructor(private _enumsService: EnumsService,
        private dialog: MatDialog,
        private _newCampaignService: NewCampaignService) {
    }

    public async ngAfterViewInit(): Promise<void> {

        this.countries = await this._enumsService.GetCountries();

        this.quillInit();

        //var ColorClass = Quill.import('attributors/class/color');
        //var SizeStyle = Quill.import('attributors/style/size');
        //Quill.register(ColorClass, true);
        //Quill.register(SizeStyle, true);

        //this.updateGeoLocationPlaceholder();
    }


    /**
   * Fire up quill wyswig editor
   */

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


    /**
     * Fires when the editor receives focus
     */
    /*
    onFocus() {
        // Add a border and reveal the toolbar
        this.element.classList.add("border-gray-200");
        this.toolbarTarget.classList.add("opacity-100");
    }
    */


    /**
     * Fires when the editor loses focus
     *
    /*
    onFocusOut() {
        // Hide the border and toolbar
        this.element.classList.remove("border-gray-200");
        this.toolbarTarget.classList.remove("opacity-100");

        // Submit the form to save our updates
        this.formTarget.requestSubmit();
    }
    */


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

    public getCountryDescription(categoryId?: number) {
        if (this.countries) {
            if (categoryId) {
                var category = this.countries.find(x => x.value == categoryId);
                if (category)
                    return category.description;
            }
        }

        return '';
    }

    /*
    public clickCountry(item: EnumDTO) {
        this.stepData.get('geoLocationCountryId').setValue(item.value);
        this.updateGeoLocationPlaceholder();
    }

    private updateGeoLocationPlaceholder() {

        const geoLocationCountry = this.stepData.get('geoLocationCountryId').value as Countries;

        switch (geoLocationCountry) {
            case Countries.Canada:
                this._geoLocation.nativeElement.placeholder = 'Please enter postal code';
                break;
            case Countries.USA:
                this._geoLocation.nativeElement.placeholder = 'Please enter zip code';
                break;
        }
    }
    */

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
