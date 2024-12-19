import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { INewsletterSubscriptionDTO } from './upraisefooter.types';
import { ResultDTO } from '../../models/ResultDTO';
@Injectable({
    providedIn: 'root'
})
export class UpraiseFooterService {

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }


    newsletter(email: string) : Observable<ResultDTO>{

        const newsletterSubscriptionDTO: INewsletterSubscriptionDTO = {
            email: email,
        };

        return this._httpClient
            .post<ResultDTO>('api/public/newsletter', newsletterSubscriptionDTO)
    }


}
