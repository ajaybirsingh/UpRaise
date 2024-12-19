import { Component, Inject, ChangeDetectorRef, OnDestroy, OnInit, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { fuseAnimations } from '@fuse/animations';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { IPublicCampaignDetail } from '../../modules/landing/home/home.types';
import { Clipboard } from '@angular/cdk/clipboard';


export interface ShareDialogData {
    publicCampaignDetail: IPublicCampaignDetail
}



@Component({
    selector: 'share',
    templateUrl: './share.dialog.html',
    styleUrls: ['./share.dialog.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations
})
export class ShareDialog implements OnInit, OnDestroy, AfterViewInit {

    public url: string;

    public showClipboardCopy: boolean = false;

    private wasShared: boolean = false;

    constructor(
        public matDialogRef: MatDialogRef<ShareDialog>,
        private clipboard: Clipboard,
        @Inject(MAT_DIALOG_DATA) public data: ShareDialogData
    ) {

        if (this.data.publicCampaignDetail.seoFriendlyURL)
            this.url = `https://app.upraise.fund/campaign-${this.data.publicCampaignDetail.seoFriendlyURL}`;
        else
            this.url = `https://app.upraise.fund/home/detail/${this.data.publicCampaignDetail.type}/${this.data.publicCampaignDetail.id}`;
    }

    public ngOnInit(): void {
    }


    public ngAfterViewInit() {
    }

    public ngOnDestroy(): void {
    }


    public close(): void {
        this.matDialogRef.close({ wasShared: this.wasShared })
    }

    public shareEmail(): void {
        this.wasShared = true;

        const subject = `Campaign for ${this.data.publicCampaignDetail.name}`;
        const body = `Hi!%0D%0A%0D%0AI thought you might interested in a campaign we are running, it is called '${this.data.publicCampaignDetail.name}'.  To learn more about this great cause visit https://app.upraise.fund/home/${this.data.publicCampaignDetail.type}/${this.data.publicCampaignDetail.id} .%0D%0A%0D%0AThanks!`;

        window.open(`mailto:?subject=${subject}&body=${body}`, '_self');
    }

    public copyLink(): void {
        this.wasShared = true;

        this.showClipboardCopy = true;

        this.clipboard.copy(this.url);

        setTimeout(() => {
            this.showClipboardCopy = false;
        }, 2000);

    }
}
