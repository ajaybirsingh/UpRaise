import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO, EnumDTO, StatusEnum } from 'app/models';
import { map, catchError, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, empty, Observable, of, throwError } from 'rxjs';
import { IPublicCampaignDetailContribution } from '../home.types';
import { IPublicClosestLocationDTO } from './explore-map.types';


@Injectable({ providedIn: 'root' })
export class ExploreMapService {

    private BASE_URL: string = 'api/public/';

    private _closestLocation: BehaviorSubject<IPublicClosestLocationDTO | null> = new BehaviorSubject(null);

    constructor(private _httpClient: HttpClient) { }

    get closestLocation$(): Observable<IPublicClosestLocationDTO> {
        return this._closestLocation.asObservable();
    }

    //https://stackoverflow.com/questions/46019771/catching-errors-in-angular-httpclient

    getClosestLocation(): Observable<IPublicClosestLocationDTO> {
        return this._httpClient.get<ResultDTO>(`${this.BASE_URL}closestlocation`).pipe(
            map((result: any) => {
                if (result.status === StatusEnum.Success && result.data) 
                    this._closestLocation.next(result.data);
                else 
                    this._closestLocation.next(null);

                return this._closestLocation.value;

            }),
            catchError((error: HttpErrorResponse) => {
                this.handleError(error);

                this._closestLocation.next(null);
                return of(null);
                //return empty();
            })
        );
    }

    private handleError(error: HttpErrorResponse) { // or it can be Error | HttpErrorResponse

        //now do console.log(error) and see what it logs
        //as per the output adjust your code

        if (error instanceof Error) {
            // Client Side Error
            console.error('Client side error:', error.error.message);
        } else if (error.message === 'failed parsing data') {
            // Client Side Processing Error
            console.error(error.message);
            return throwError(error.message);
        } else {
            // Service Side Error
            console.error(`Server returned code ${error.status}, ` + `body was: ${error.error}`);
        }

        //return an observable error message
        //return throwError('failed to contact server');
    }

    /*
    public getClosestLocation(): Observable<IPublicClosestLocationDTO> {

        return this._httpClient.get<ResultDTO>(`${this.BASE_URL}closestlocation`).pipe(
            map((result) => {

                // Update the course
                this._closestLocation.next(result.data);

                // Return the course
                return result.data;
            }),
            switchMap((result) => {

                if (!result) {
                    return throwError(`Could not fetch closest location`);
                }

                return of(result.data);
            })
        );
    }
    */
}
