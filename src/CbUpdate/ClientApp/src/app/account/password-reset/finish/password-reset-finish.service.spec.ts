import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ApplicationConfigService } from 'app/core/config/application-config.service';
import { PasswordResetFinishService } from './password-reset-finish.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('PasswordResetFinish Service', () => {
  let service: PasswordResetFinishService;
  let httpMock: HttpTestingController;
  let applicationConfigService: ApplicationConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});

    service = TestBed.inject(PasswordResetFinishService);
    applicationConfigService = TestBed.inject(ApplicationConfigService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Service methods', () => {
    it('should call reset-password/finish endpoint with correct values', () => {
      // GIVEN
      const key = 'abc';
      const newPassword = 'password';

      // WHEN
      service.save(key, newPassword).subscribe();

      const testRequest = httpMock.expectOne({
        method: 'POST',
        url: applicationConfigService.getEndpointFor('api/account/reset-password/finish'),
      });

      // THEN
      expect(testRequest.request.body).toEqual({ key, newPassword });
    });
  });
});
