import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { ResultDTO } from '../../../../models';
import { CampaignTypes } from '../../../../shared/enums';
import { CampaignAnalyticTypes } from './details.enums';
import { ICampaignAnalyticRequestDTO, ICampaignFollowDTO } from './details.types';
@Injectable({
    providedIn: 'root'
})
export class DetailService {

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }


    addAnalytics(id: number, campaignType: CampaignTypes, analyticType: CampaignAnalyticTypes) {
        const campaignAnalyticRequestDTO: ICampaignAnalyticRequestDTO = {
            id: id,
            campaignType: campaignType,
            analyticType: analyticType
        };

        this._httpClient
            .post('api/public/analytic', campaignAnalyticRequestDTO)
            .subscribe();
    }

    followCampaign(id: number, campaignType: CampaignTypes, isFollowing: boolean): Observable<ResultDTO> {
        const campaignAnalyticRequestDTO: ICampaignFollowDTO = {
            id: id,
            campaignType: campaignType,
            isFollowing: isFollowing
        };

        return this._httpClient
            .post<ResultDTO>('api/campaigns/follow', campaignAnalyticRequestDTO);
    }

}
