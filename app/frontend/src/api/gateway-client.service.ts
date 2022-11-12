import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { catchError, filter, map, Observable, of, switchMap, tap } from 'rxjs';
import { HttpOptions, TokensDto } from './api.models';
import { getHandledResponse, getTokens, getTransformHeaders } from './api.helpers';
import { Router } from '@angular/router';
import { setCookie } from '../app/common/functions';

@Injectable({
  providedIn: 'root'
})
export class GatewayClientService {
  constructor(private readonly http: HttpClient,
              private readonly router: Router) {}

  get<T>(commandUrl: string, options?: HttpOptions): Observable<T> {
    return this.http.get<T>(commandUrl, {
      ...options,
      observe: 'response',
      headers: getTransformHeaders(options)
    }).pipe(
      catchError((response: HttpResponse<T>) => {
        const status = response.status;
        if (status === 401) {
          return this.refreshTokenAndResendRequest().pipe(
            switchMap((result) => {
              if (result) {
                return this.http.get<T>(commandUrl, {
                  ...options,
                  observe: 'response',
                  headers: getTransformHeaders(options)
                });
              }
              return of(null);
            })
          );
        }
        return of(null);
      }),
      filter((response): response is HttpResponse<T> => !!response),
      map((response) => getHandledResponse(response))
    );
  }

  post<T>(commandUrl: string, body?: any | null, options?: HttpOptions): Observable<T> {
    return this.http.post<T>(commandUrl, body, {
      ...options,
      observe: 'response',
      headers: getTransformHeaders(options)
    }).pipe(
      catchError((response) => {
        const status = response.status;
        if (status === 401) {
          return this.refreshTokenAndResendRequest().pipe(
            switchMap((result) => {
              if (result) {
                return this.http.post<T>(commandUrl, {
                  ...options,
                  observe: 'response',
                  headers: getTransformHeaders(options)
                });
              }
              return of(null);
            })
          );
        }
        return of(null);
      }),
      filter((response): response is HttpResponse<T> => !!response),
      map((response) => getHandledResponse(response))
    );
  }

  patch<T>(commandUrl: string, body: any | null, options?: HttpOptions): Observable<T> {
    return this.http.patch<T>(commandUrl, body, {
      ...options,
      observe: 'response',
      headers: getTransformHeaders(options)
    }).pipe(
      catchError((response) => {
        const status = response.status;
        if (status === 401) {
          return this.refreshTokenAndResendRequest().pipe(
            switchMap((result) => {
              if (result) {
                return this.http.patch<T>(commandUrl, {
                  ...options,
                  observe: 'response',
                  headers: getTransformHeaders(options)
                });
              }
              return of(null);
            })
          );
        }
        return of(null);
      }),
      filter((response): response is HttpResponse<T> => !!response),
      map((response) => getHandledResponse(response))
    );
  }

  private refreshTokenAndResendRequest<T>(): Observable<TokensDto | null> {
    return this.http.post<TokensDto>('/api/auth/refresh', getTokens(), {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    }).pipe(
      catchError(() => {
        this.router.navigate(['/auth']).then();
        return of(null);
      }),
      tap((tokensDto: TokensDto | null) => {
        if (tokensDto) {
          setCookie('accessToken', tokensDto.accessToken, 10);
          setCookie('refreshToken', tokensDto.refreshToken, 10);
        }
      })
    );
  }
}
