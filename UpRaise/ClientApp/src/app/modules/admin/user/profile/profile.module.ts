import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { ProfileComponent } from 'app/modules/admin/user/profile/profile.component';
import { ImageCropperModule } from 'ngx-image-cropper';
import { MatTabsModule } from '@angular/material/tabs';
import { AccountComponent } from 'app/modules/admin/user/profile/tabs/account/account.component';
import { PasswordComponent } from 'app/modules/admin/user/profile/tabs/password/password.component';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'app/shared/shared.module';
import { TranslocoModule } from '@ngneat/transloco';
import { MatDialogModule } from '@angular/material/dialog';
import { ImageSelectorDialog } from 'app/components/image-selector/image-selector.dialog';
import { FuseCardModule } from '@fuse/components/card';
import { FuseAlertModule } from '@fuse/components/alert';
import { MatButtonModule } from '@angular/material/button';
import { SharedPipesModule } from 'app/core/pipes/SharedPipesModule'

const exampleRoutes: Route[] = [
    {
        path     : '',
        component: ProfileComponent
    }
];

@NgModule({
    declarations: [
        ProfileComponent,
        AccountComponent,
        PasswordComponent,
        ImageSelectorDialog
    ],
    imports     : [
        RouterModule.forChild(exampleRoutes),
        MatIconModule,
        MatButtonModule,
        MatFormFieldModule,
        MatTabsModule,
        MatDialogModule,
        ReactiveFormsModule,
        ImageCropperModule,
        TranslocoModule,
        SharedModule,
        FuseCardModule,
        SharedPipesModule,
        FuseAlertModule
    ]
})
export class ProfileModule
{
}
