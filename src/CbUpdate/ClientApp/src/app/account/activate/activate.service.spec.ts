import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ApplicationConfigService } from 'app/core/config/application-config.service';
import { ActivateService } from './activate.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('ActivateService Service', () => {
  let service: ActivateService;
  let httpMock: HttpTestingController;
  let applicationConfigService: ApplicationConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});

    service = TestBed.inject(ActivateService);
    applicationConfigService = TestBed.inject(ApplicationConfigService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Service methods', () => {
    it('should call api/activate endpoint with correct values', () => {
      // GIVEN
      let expectedResult;
      const key = 'key';
      const value = true;

      // WHEN
      service.get(key).subscribe(received => {
        expectedResult = received;
      });
      const testRequest = httpMock.expectOne({
        method: 'GET',
        url: applicationConfigService.getEndpointFor(`api/activate?key=${key}`),
      });
      testRequest.flush(value);

      // THEN
      expect(expectedResult).toEqual(value);
    });
  });
});
