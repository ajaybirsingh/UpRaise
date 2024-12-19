import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { ResultDTO } from '../../models/ResultDTO';
@Injectable({
    providedIn: 'root'
})
export class UpraiseHeaderService {

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }


}
