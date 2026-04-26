import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VersionCompare } from './version-compare';

describe('VersionCompare', () => {
  let component: VersionCompare;
  let fixture: ComponentFixture<VersionCompare>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VersionCompare]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VersionCompare);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
