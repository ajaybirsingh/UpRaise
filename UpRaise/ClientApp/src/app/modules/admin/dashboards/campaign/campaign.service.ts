import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { IDashboardActivityRequestDTO, IDashboardActivityResponseDTO, IDashboardCampaignRequestDTO, IDashboardCampaignResponseDTO, IDashboardCampaignsResponseDTO } from './campaign.types';
import { ResultDTO } from '../../../../models';


@Injectable({
    providedIn: 'root'
})
export class CampaignService {
    private _data: BehaviorSubject<any> = new BehaviorSubject(null);
    private _campaigns: BehaviorSubject<any> = new BehaviorSubject(null);
    private _activities: BehaviorSubject<any> = new BehaviorSubject(null);

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for data
     */
    get campaigns$(): Observable<any> {
        return this._campaigns.asObservable();
    }


    get data$(): Observable<any> {
        return this._data.asObservable();
    }

    get activities$(): Observable<any> {
        return this._activities.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get data
     */
    getCampaigns(): Observable<IDashboardCampaignsResponseDTO[]> {
        return this._httpClient.get('api/dashboards/getcampaigns').pipe(
            tap((response: any) => {
                this._campaigns.next(response.data);
            })
        );
    }


    getData(): Observable<any> {
        return this._httpClient.get('api/dashboards/project').pipe(
            tap((response: any) => {
                this._data.next(projectData);
                //this._data.next(response);
            })
        );
    }



    getCampaign(dashboardCampaignRequestDTO: IDashboardCampaignRequestDTO): Observable<IDashboardCampaignResponseDTO> {
        return this._httpClient
            .post<IDashboardCampaignResponseDTO>('api/dashboards/campaign', dashboardCampaignRequestDTO)
            .pipe(
                map((dashboardCampaignResponseDTO: IDashboardCampaignResponseDTO) => {


                    if (dashboardCampaignResponseDTO.startDate)
                        dashboardCampaignResponseDTO.startDate = new Date(dashboardCampaignResponseDTO.startDate)

                    if (dashboardCampaignResponseDTO.endDate)
                        dashboardCampaignResponseDTO.endDate = new Date(dashboardCampaignResponseDTO.endDate)

                    return dashboardCampaignResponseDTO;
                }));
    }


    /**
    * Get activities
    */
    getActivities(dashboardActivityRequestDTO: IDashboardActivityRequestDTO): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/dashboards/activities', dashboardActivityRequestDTO)
            .pipe(tap((resultDTO: ResultDTO) => {

                if (resultDTO)
                    this._activities.next(resultDTO.data);
            })
            );
    }
}
