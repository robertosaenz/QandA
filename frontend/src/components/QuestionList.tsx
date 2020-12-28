import React, { FC, memo } from 'react';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { gray5, accent2 } from '../style/Styles';

// Extra Components
import Question from './Question';

// Interfaces
import { QuestionsData } from '../interfaces/QuestionsData';

interface Props {
  QuestionsData: QuestionsData[];
  renderItem?: (item: QuestionsData) => JSX.Element; //IF I WANT ONE PARAMETER CALLED WITH FUNCTION
}
const QuestionList: FC<Props> = memo(({ QuestionsData, renderItem }) => {
  console.log('Rendering QuestionList', QuestionsData, renderItem);

  return (
    <ul
      css={css`
        list-style: none;
        margin: 10px 0 0 0;
        padding: 0px 20px;
        background-color: #fff;
        border-bottom-left-radius: 4px;
        border-bottom-right-radius: 4px;
        border-top: 3px solid ${accent2};
        box-shadow: 0 3px 5px 0 rgba(0, 0, 0, 0.16);
      `}
    >
      {QuestionsData.map((question) => (
        <li
          key={question.questionId}
          css={css`
            border-top: 1px solid ${gray5};
            :first-of-type {
              border-top: none;
            }
          `}
        >
          {/* IF I WANT ONE PARAMETER CALLED WITH FUNCTION */}
          {renderItem ? (
            renderItem(question)
          ) : (
            <Question QuestionsData={question} />
          )}
        </li>
      ))}
    </ul>
  );
});

export default QuestionList;
