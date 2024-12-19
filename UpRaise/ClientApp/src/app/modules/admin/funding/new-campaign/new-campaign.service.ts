import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { IEditCampaign, IGeocodingRequest, IGeocodingResponse } from './new-campaign.types';
import { ResultDTO } from '../../../../models';

@Injectable({
    providedIn: 'root'
})
export class NewCampaignService {

    private BASE_URL: string = 'api/campaigns/';

    private _editCampaign: BehaviorSubject<IEditCampaign | null> = new BehaviorSubject(null);

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }

    get editCampaign$(): Observable<IEditCampaign> {
        return this._editCampaign.asObservable();
    }

    public getEditCampaignById(id: string): Observable<IEditCampaign> {

        return this._httpClient.get<IEditCampaign>(`${this.BASE_URL}getEditCampaign`, { params: { id } }).pipe(
            map((campaignRedline) => {

                // Update the course
                this._editCampaign.next(campaignRedline);

                // Return the course
                return campaignRedline;
            }),
            switchMap((campaignRedline) => {

                if (!campaignRedline) {
                    return throwError(`Could not fetch edit campaign (${id})!`);
                }

                return of(campaignRedline);
            })
        );
    }


    public geocode(location: string): Observable<ResultDTO> {

        var geocodeRequest: IGeocodingRequest = {
            location: location
        };

        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}geocoding`, geocodeRequest).pipe(
            map((resultDTO) => {

                // Update the course
                //this._editCampaign.next(campaignRedline);

                // Return the course
                return resultDTO;
            })/*,
            switchMap((campaignRedline: any) => {

                if (!campaignRedline) {
                    return throwError(`Could not fetch edit campaign (${id})!`);
                }

                return of(campaignRedline);
            })
            */
        );
    }

}
