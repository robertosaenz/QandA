import React, { FC } from 'react';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { PageTitle } from '../style/PageTitle';

interface Props {
  title?: string;
}

const Page: FC<Props> = ({ title, children }) => {
  // console.log(title);
  return (
    <div
      css={css`
        margin: 50px auto 20px auto;
        padding: 30px 20px;
        max-width: 600px;
      `}
    >
      {title && <PageTitle>{title}</PageTitle>}
      {children}
    </div>
  );
};

export default Page;
