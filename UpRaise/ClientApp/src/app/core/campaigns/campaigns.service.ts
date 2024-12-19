import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, ReplaySubject } from 'rxjs';
import { IUser } from 'app/models/user';
import { retry, catchError, switchMap } from 'rxjs/operators';
import { ResultDTO } from 'app/models';
import { map } from 'rxjs/operators';
import { IEmailBeneficiaryRequestDTO } from '../../modules/admin/dashboards/campaign/campaign.types';

@Injectable({
    providedIn: 'root'
})

export class CampaignService
{
    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient)
    {
    }

    public saveCampaign(campaignData: any): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/campaigns/saveCampaign', campaignData);
    }


    public emailBeneficiary(emailBeneficiaryRequestDTO: IEmailBeneficiaryRequestDTO): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/campaigns/emailBeneficiary', emailBeneficiaryRequestDTO);
    }
    
    /*
    public getYourCampaigns(state: DataSourceRequestState): Observable<DataResult> {
        const queryStr = `${toDataSourceRequestString(state)}`; // Serialize the state
        const hasGroups = state.group && state.group.length;

        return this._httpClient
            .get(`api/campaigns/getYourCampaigns?${queryStr}`)// Send the state to the server
            .pipe(map(({ data, total }: GridDataResult) => // Process the response
            (<GridDataResult>{
                // If there are groups, convert them to a compatible format
                data: hasGroups ? translateDataSourceResultGroups(data) : data,
                total: total,
                // Convert the aggregates if such exist
                //aggregateResult: translateAggregateResults(aggregateResults)
            })
            ));
    }
*/



}
