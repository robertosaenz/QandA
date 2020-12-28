import React, { FC } from 'react';
/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { gray3 } from '../style/Styles';

// Interfaces
import { AnswersData } from '../interfaces/AnswersData';

interface Props {
  AnswersData: AnswersData;
}

const Answer: FC<Props> = ({ AnswersData }) => {
  return (
    <div
      css={css`
        padding: 10px 0px;
      `}
    >
      <div
        css={css`
          padding: 10px 0px;
          font-size: 13px;
        `}
      >
        {AnswersData.content}
      </div>
      <div
        css={css`
          font-size: 12px;
          font-style: italic;
          color: ${gray3};
        `}
      >
        {`Answered by ${AnswersData.userName} on
    ${AnswersData.created.toLocaleDateString()}
    ${AnswersData.created.toLocaleTimeString()}`}
      </div>
    </div>
  );
};

export default Answer;
