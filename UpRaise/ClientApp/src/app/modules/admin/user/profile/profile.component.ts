import { Component, ViewEncapsulation } from '@angular/core';
import { ProfileTypes } from './profile.types';

@Component({
    selector     : 'profile',
    templateUrl  : './profile.component.html',
    encapsulation: ViewEncapsulation.None
})
export class ProfileComponent
{

    public ProfileTypes = ProfileTypes;

    public selectedProfileType: ProfileTypes = ProfileTypes.Account;

    /**
     * Constructor
     */
    constructor()
    {
    }
}
