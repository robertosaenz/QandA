import React, { FC } from 'react';
// Interface
import { AnswersData } from '../interfaces/AnswersData';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import Answer from './Answer';
import { gray5 } from '../style/Styles';

interface Props {
  AnswersData: AnswersData[];
}

const AnswerList: FC<Props> = ({ AnswersData }) => {
  return (
    <ul
      css={css`
        list-style: none;
        margin: 10px 0 0 0;
        padding: 0;
      `}
    >
      {AnswersData.map((answer) => (
        <li
          css={css`
            border-top: 1px solid ${gray5};
          `}
          key={answer.answerId}
        >
          <Answer AnswersData={answer} />
        </li>
      ))}
    </ul>
  );
};

export default AnswerList;
