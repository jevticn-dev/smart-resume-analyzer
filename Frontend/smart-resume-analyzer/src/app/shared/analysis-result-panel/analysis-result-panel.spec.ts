import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnalysisResultPanel } from './analysis-result-panel';

describe('AnalysisResultPanel', () => {
  let component: AnalysisResultPanel;
  let fixture: ComponentFixture<AnalysisResultPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnalysisResultPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AnalysisResultPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
