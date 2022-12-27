import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, NgZone } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { BehaviorSubject, map, Observable, of, Subject, switchMap, take, takeUntil, tap } from 'rxjs';
import { NotificationSettingsDto, UserDto } from '../../../api/api.models';
import { ApiService } from '../../../api/api.service';
import { ACCESS_TOKEN_COOKIE_NAME, REFRESH_TOKEN_COOKIE_NAME } from '../../common/constants';
import { deleteCookie } from '../../common/functions';
import { LoaderModule } from '../loader/loader.module';
import { SettingsModalComponent } from '../settings-modal/settings-modal.component';
import { SvgIconComponent } from '../svg-icon/svg-icon.component';

@Component({
  selector: 'app-header',
  standalone: true,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    NzPageHeaderModule,
    NzAvatarModule,
    SvgIconComponent,
    NzDropDownModule,
    RouterLink,
    NzIconModule,
    NzModalModule,
    SettingsModalComponent,
    LoaderModule
  ],
})
export class HeaderComponent {
  @Input() user$: Observable<UserDto | null> = of(null);

  readonly defaultSettings: NotificationSettingsDto = {
    periodInDays: 0
  };

  readonly loading$ = new BehaviorSubject<boolean>(false);
  readonly isSettingsModalOpen$ = new BehaviorSubject<boolean>(false);

  settings$ = new BehaviorSubject<NotificationSettingsDto | null>(null);

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly router: Router,
              private readonly ngZone: NgZone,
              private readonly fb: FormBuilder,
              private readonly apiService: ApiService) {}

  ngOnInit(): void {
    this.isSettingsModalOpen$.pipe(
      tap((isModalOpen) => this.loading$.next(isModalOpen)),
      switchMap((isModalOpen) => isModalOpen
        ? this.user$.pipe(
          switchMap((user) => user ? this.apiService.getNotificationSettings(user.id) : of(null)),
          map((settings) => settings ?? this.defaultSettings),
        )
        : of(null)
      ),
      takeUntil(this.destroy$)
    ).subscribe((settings) => {
      this.settings$.next(settings);
      this.loading$.next(false);
    });
  }

  logout(): void {
    deleteCookie(ACCESS_TOKEN_COOKIE_NAME);
    deleteCookie(REFRESH_TOKEN_COOKIE_NAME);
    this.ngZone.run(() => this.router.navigate(['auth']).then(() => window.location.reload()));
  }

  saveSettings(settings: NotificationSettingsDto): void {
    this.user$.pipe(
      switchMap((user) => this.apiService.saveNotificationSettings(user!.id, settings)),
      take(1),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.isSettingsModalOpen$.next(false);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
