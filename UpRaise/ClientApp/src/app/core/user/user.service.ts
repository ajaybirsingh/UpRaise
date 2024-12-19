import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { IUser, IUserNotifications, IUserPersonalInformation } from 'app/models/user';
import { retry, catchError, switchMap } from 'rxjs/operators';
import { ResultDTO } from 'app/models';

@Injectable({
    providedIn: 'root'
})
export class UserService
{
    private BLANK_PROFILE_URL: string = 'assets/images/avatars/profile.jpg';

    private _user: ReplaySubject<IUser> = new ReplaySubject<IUser>(1);

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient)
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Setter & getter for user
     *
     * @param value
     */
    set user(value: IUser)
    {
        // Store the value
        this._user.next(value);
    }

    get user$(): Observable<IUser>
    {
        return this._user.asObservable();
    }


    public refreshUser():any {

        this._httpClient.get(`api/users/user`).pipe(
            retry(1),

            /*
            .map((response: Response) => {
                console.log(response.json());
                response.json();
            }))
            .subscribe();
            */
            catchError(() =>

                // Return false
                of(false)
            ),
            switchMap((response: any) => {

                this._user.next(response);

                // Return true
                return of(true);
            })
        ).subscribe();


                
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------
    public updateProfile(userEditProfileDTO: { firstName: string; lastName: string }): Observable<ResultDTO>
    {
        return this._httpClient.post<ResultDTO>('api/users/editProfile', userEditProfileDTO);
    }

    
    public sendMessage(userMessageDTO: { toUserAliasId: string, fullName: string; subject: string, message: string  }): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/users/sendMessage', userMessageDTO);
    }

    public resetPassword(userEditPasswordDTO: { oldPassword: string; newPassword: string }): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/users/resetPassword', userEditPasswordDTO);
    }


    public getNotifications(): Observable<ResultDTO> {
        return this._httpClient.get<ResultDTO>('api/users/notifications');
    }

    public updateNotifications(userNotifications: IUserNotifications): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/users/notifications', userNotifications);
    }

    public getPersonalInformation(): Observable<ResultDTO> {
        return this._httpClient.get<ResultDTO>('api/users/personalinformation');
    }

    public updatePersonalInformation(personalInformation: IUserPersonalInformation): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/users/personalinformation', personalInformation);
    }

    public getProfile(): Observable<ResultDTO> {
        return this._httpClient.get<ResultDTO>('api/users/profile');
    }

    /*
    public updateProfile(userNotifications: IUserNotifications): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>('api/users/profile', userNotifications);
    }
    */

    public getBlankProfileURL(): string {
        return this.BLANK_PROFILE_URL;
    }

    public getProfilePictureURL(aliasId: string, updatedAt: string): string {
        var url = "api/file/getprofilepicture/" + aliasId;
        
        if (updatedAt && updatedAt.length > 0) {
            var dt = new Date(Date.parse(updatedAt));
            url = url + "?" + dt.getTime();
        }

        return url;
    }

    //function parseDate(str_date) {
        //return new Date(Date.parse(str_date));
    //}
}
