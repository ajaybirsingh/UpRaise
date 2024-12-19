/* eslint-disable max-len */
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'changelog',
    templateUrl: './changelog.html',
    styles: [''],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangelogComponent {
    changelog: any[] = [

        // v0.2
        {
            version: 'v0.2',
            releaseDate: 'June 4, 2021',
            changes: [
                {
                    type: 'Added',
                    list: [
                        'Redis/SQL Azure'
                    ]
                },
                {
                    type: 'Changed',
                    list: [
                        'Updated the user interface',
                    ]
                }
            ]
        },
        // v0.1
        {
            version: 'v0.1',
            releaseDate: 'May 31, 2021',
            changes: [
                {
                    type: 'Added',
                    list: [
                        'Building out infrastructure'
                    ]
                },
            ]
        },
    ];

    /**
     * Constructor
     */
    constructor() {
    }
}
