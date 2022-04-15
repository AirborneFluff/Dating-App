import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs/operators';
import random, { Random } from 'random';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busySerivce: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busySerivce.busy();
    return next.handle(request).pipe(
      //delay(random.int(100, 320)),
      finalize(() => {
        this.busySerivce.idle();
      })
    );
  }
}
