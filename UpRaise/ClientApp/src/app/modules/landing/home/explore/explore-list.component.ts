import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import * as moment from 'moment';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { DOCUMENT } from '@angular/common';
import { MatTabGroup } from '@angular/material/tabs';
import { BehaviorSubject, combineLatest, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { IPublicCampaign, IPublicCampaignDetail } from '../home.types';
import { HomeService } from '../home.service';
import { EnumDTO } from '../../../../models';
import { EnumsService } from '../../../../core/services/enums.service';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../../core/user/user.service';
import { ExploreListService } from './explore-list.service';

@Component({
    selector: 'explore-list',
    templateUrl: './explore-list.component.html',
    styleUrls: ['./explore-list.component.css'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExploreListComponent implements OnInit, OnDestroy {

    @Input() public campaigns: IPublicCampaign[];

    /**
     * Constructor
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
    ) {

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
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    dollarFormatter(dollarValue?: number): string {

        if (!dollarValue)
            return "";

        var formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            // These options are needed to round to whole numbers if that's what you want.
            //minimumFractionDigits: 0, // (this suffices for whole numbers, but will print 2500.10 as $2,500.1)
            //maximumFractionDigits: 0, // (causes 2500.99 to be printed as $2,501)
        });

        var strValue = formatter.format(dollarValue);
        return strValue;

    }

    getRelativeFormat(date: string): string {
        const today = moment().startOf('day');
        const yesterday = moment().subtract(1, 'day').startOf('day');

        // Is today?
        if (moment(date, moment.ISO_8601).isSame(today, 'day')) {
            return 'Today';
        }

        // Is yesterday?
        if (moment(date, moment.ISO_8601).isSame(yesterday, 'day')) {
            return 'Yesterday';
        }

        return moment(date, moment.ISO_8601).fromNow();
    }


}



