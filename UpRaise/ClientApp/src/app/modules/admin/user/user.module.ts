import { NgModule } from '@angular/core';

import { ProfileModule } from 'app/modules/admin/user/profile/profile.module';
//import { MessagesModule } from 'app/modules/admin/user/messages/messages.module';


@NgModule({
  imports: [
    ProfileModule,
    //MessagesModule
  ],

})
export class UserModule {

}
