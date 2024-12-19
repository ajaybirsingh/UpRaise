import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { ICampaignRedline, ICampaignRedlineCommentRequest, ICampaignRedlineStatusDTO } from './campaign-redlining.types';
import { ResultDTO } from '../../../models';

@Injectable({
    providedIn: 'root'
})
export class CampaignRedliningService {

    private BASE_URL: string = 'api/campaigns/';

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }

    public getCampaignRedlineById(id: string): Observable<ICampaignRedline> {

        return this._httpClient.get<ICampaignRedline>(`${this.BASE_URL}redline`, { params: { id } }).pipe(
            map((campaignRedline) => {
                // Return the course
                return campaignRedline;
            }),
            switchMap((campaignRedline) => {
                if (!campaignRedline) {
                    return throwError(`Could not found campaign redline (${id})!`);
                }
                return of(campaignRedline);
            })
        );
    }


    public addRedlineComment(campaignRedlineCommentRequest: ICampaignRedlineCommentRequest): Observable<ResultDTO> {
        return this._httpClient
            .post<ResultDTO>(`${this.BASE_URL}redlinecomment`, campaignRedlineCommentRequest);
    }

    public updateStatus(campaignRedlineStatusDTO: ICampaignRedlineStatusDTO): Observable<ResultDTO> {
        return this._httpClient
            .post<ResultDTO>(`${this.BASE_URL}redlinestatus`, campaignRedlineStatusDTO);
    }

}
