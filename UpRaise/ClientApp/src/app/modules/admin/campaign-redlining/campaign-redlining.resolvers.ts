import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { EnumDTO, ResultDTO } from 'app/models';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../../core/auth/auth.service';
import { EnumsService } from '../../../core/services/enums.service';
import { CampaignRedliningService } from './campaign-redlining.service';
import { ICampaignRedline } from './campaign-redlining.types';


@Injectable({
    providedIn: 'root'
})
export class CampaignRedliningResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(
        private _router: Router,
        private _campaignRedliningService: CampaignRedliningService
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Resolver
     *
     * @param route
     * @param state
     */
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<ICampaignRedline> {
        const id = route.paramMap.get('id');
        
        return this._campaignRedliningService.getCampaignRedlineById(id)
            .pipe(
                // Error here means the requested task is not available
                catchError((error) => {

                    // Log the error
                    console.error(error);

                    // Get the parent url
                    //const parentUrl = state.url.split('/').slice(0, -2).join('/');

                    // Navigate to there
                    this._router.navigateByUrl('/home');

                    // Throw an error
                    return throwError(error);
                })
            );
            
    }

   

}
